using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SaveSystem {
    /// <summary>
    /// Highest level for save/load system: manages profiles, slots, and disk I/O.
    /// </summary>
    [DefaultExecutionOrder(-1500)]
    public class SaveManager : MonoBehaviour {
        private const string TAG = "SaveManager";
        private const string AutoSaveSlotId = "autosave";

        public static SaveManager Instance { get; private set; }

        [SerializeField] private string saveFolderName = "Saves";

        private string RootFolder => Path.Combine(Application.persistentDataPath, saveFolderName);
        private string ProfilesFolder => Path.Combine(RootFolder, "Profiles");
        private string ProfileFolder(string profileId) => Path.Combine(ProfilesFolder, profileId);
        private string ProfileIndexPath(string profileId) => Path.Combine(ProfileFolder(profileId), "index.json");
        private string ProfileSlotsFolder(string profileId) => Path.Combine(ProfileFolder(profileId), "Slots");
        private string SlotPath(string profileId, string slotId) => Path.Combine(ProfileSlotsFolder(profileId), slotId + ".json");

        public string ActiveProfileId { get; private set; }
        public SaveProfile ActiveProfile { get; private set; }
        public bool IsBusy { get; private set; }
        public bool HasAutoSave => !string.IsNullOrEmpty(ActiveProfile?.autoSave?.slotId);
        public bool CanContinue() => ListProfiles().Any(p => !string.IsNullOrEmpty(p.autoSave?.slotId) || p.manualSaves.Count > 0);
    

        public event Action<string> SaveCompleted;       // slotId
        public event Action<string, string> SaveFailed;   // slotId, reason
        public event Action<string> LoadCompleted;        // slotId
        public event Action<string, string> LoadFailed;   // profileId or slotId, reason

        private SaveSlotData pendingLoadData;
        private string pendingLoadSlotId;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            SaveFileIO.EnsureFolder(ProfilesFolder);
            GameLog.Log(TAG, "Initialized");
        }

        private void Start() => SceneLoader.Instance.ContentLoaded += HandleContentLoaded;
        private void OnDestroy() { 
            if (SceneLoader.Instance != null) 
                SceneLoader.Instance.ContentLoaded -= HandleContentLoaded; 
        }

        // ---------- Profile CRUD ----------

        /// <summary>
        /// Scans disk for every profile folder and reads its index.
        /// </summary>
        public List<SaveProfile> ListProfiles() {
            SaveFileIO.EnsureFolder(ProfilesFolder);
            var result = new List<SaveProfile>();

            foreach (var dir in Directory.GetDirectories(ProfilesFolder)) {
                var profileId = Path.GetFileName(dir);
                var indexPath = ProfileIndexPath(profileId);
                if (!File.Exists(indexPath))
                    continue;

                var profile = SaveFileIO.ReadIndex(indexPath);
                if (string.IsNullOrEmpty(profile.profileId)) {
                    GameLog.Warning(TAG, $"Skipping unreadable/corrupt profile folder '{profileId}'.");
                    continue;
                }

                result.Add(profile);
            }

            return result;
        }

        /// <summary>
        /// Creates a new profile with prompted display name.
        /// </summary>
        public SaveProfile CreateProfile(string displayName) {
            var profileId = Guid.NewGuid().ToString("N");
            var now = DateTime.UtcNow.Ticks;

            var profile = new SaveProfile {
                profileId = profileId,
                displayName = displayName,
                createdUtcTicks = now,
                updatedUtcTicks = now
            };

            SaveFileIO.EnsureFolder(ProfileSlotsFolder(profileId));
            SaveFileIO.WriteIndex(ProfileIndexPath(profileId), profile);

            ActiveProfileId = profileId;
            ActiveProfile = profile;

            GameLog.Log(TAG, $"Created profile '{profileId}' ('{displayName}')");
            return profile;
        }

        /// <summary>
        /// Updates display name of existing profile. Does not change active profile unless the renamed profile is currently active.
        /// </summary>
        public void RenameProfile(string profileId, string displayName) {
            var path = ProfileIndexPath(profileId);
            var profile = SaveFileIO.ReadIndex(path);
            if (string.IsNullOrEmpty(profile.profileId)) {
                GameLog.Warning(TAG, $"RenameProfile: profile '{profileId}' not found.");
                return;
            }

            profile.displayName = displayName;
            profile.updatedUtcTicks = DateTime.UtcNow.Ticks;
            SaveFileIO.WriteIndex(path, profile);

            if (ActiveProfileId == profileId)
                ActiveProfile = profile;
        }

        /// <summary>
        /// Deletes profile with all related data.
        /// </summary>
        public void DeleteProfile(string profileId) {
            var dir = ProfileFolder(profileId);
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);

            if (ActiveProfileId == profileId) {
                ActiveProfileId = null;
                ActiveProfile = null;
            }

            GameLog.Log(TAG, $"Deleted profile '{profileId}'");
        }

        private void OpenProfile(string profileId) {
            ActiveProfileId = profileId;
            ActiveProfile = SaveFileIO.ReadIndex(ProfileIndexPath(profileId));
        }

        // ---------- Save / load within the active profile ----------

        /// <summary>
        /// Saves current game state to a auto-save slot in active profile.
        /// </summary>
        public void SaveAuto() {
            if (!EnsureActiveProfile()) 
                return;
            SaveInternal(AutoSaveSlotId, "Auto Save", isAuto: true);
        }

        /// <summary>
        /// Create a new manual save slot in active profile.  
        /// Then save current game state to created slot in active profile. Return new slotId or null if failed. 
        /// </summary>
        public string SaveManual(string displayName) {
            if (!EnsureActiveProfile())
                return null;
            var slotId = Guid.NewGuid().ToString("N");
            SaveInternal(slotId, displayName, isAuto: false);
            return slotId;
        }

        public void OverwriteManual(string slotId) {
            if (!EnsureActiveProfile()) 
                return;
            if (ActiveProfile.manualSaves.All(m => m.slotId != slotId)) {
                GameLog.Warning(TAG, $"OverwriteManual: no existing manual slot '{slotId}' in profile '{ActiveProfileId}'.");
                return;
            }
            SaveInternal(slotId, null, isAuto: false);
        }

        /// <summary>
        /// Deletes a manual save from any profile, active or not.
        /// </summary>
        public void DeleteManualSave(string profileId, string slotId)
        {
            var indexPath = ProfileIndexPath(profileId);
            var profile = SaveFileIO.ReadIndex(indexPath);
            var meta = profile.manualSaves.FirstOrDefault(m => m.slotId == slotId);
            if (meta == null)
                return;

            SaveFileIO.DeleteSlot(SlotPath(profileId, slotId));
            profile.manualSaves.Remove(meta);
            SaveFileIO.WriteIndex(indexPath, profile);

            if (ActiveProfileId == profileId)
                ActiveProfile = profile;
        }

        /// <summary>
        /// Loads a specific save slot from a specific profile: reads it from disk, gets its
        /// scene active via SceneLoader (reusing the current scene if it already matches), then
        /// restores state onto every registered ISaveable once that scene is active. Becomes the
        /// active profile as a side effect.
        /// </summary>
        public void LoadSlot(string profileId, string slotId) {
            if (IsBusy) { 
                GameLog.Warning(TAG, "LoadSlot ignored: SaveManager busy");
                return;
            }

            if (SceneLoader.Instance.IsBusy) {
                GameLog.Warning(TAG, "LoadSlot ignored: SceneLoader busy"); 
                return; 
            }

            var data = SaveFileIO.ReadSlot(SlotPath(profileId, slotId));
            if (data == null) {
                GameLog.Error(TAG, $"LoadSlot('{profileId}'/'{slotId}') failed: slot file missing or unreadable");
                LoadFailed?.Invoke(slotId, "Slot file missing or unreadable");
                return;
            }

            OpenProfile(profileId);

            IsBusy = true;
            pendingLoadData = data;
            pendingLoadSlotId = slotId;

            GameStateManager.Instance.SetPauseReason(TAG, true);
            GameStateManager.Instance.SetMode(GameMode.Loading);
            SceneLoader.Instance.LoadContent(data.sceneName);
        }

        /// <summary>
        /// Loads whichever save (auto or manual) was updated most recently in this profile.
        /// </summary>
        public void ContinueProfile(string profileId)
        {
            var profile = SaveFileIO.ReadIndex(ProfileIndexPath(profileId));
            if (string.IsNullOrEmpty(profile.profileId))
            {
                GameLog.Error(TAG, $"ContinueProfile: profile '{profileId}' not found.");
                LoadFailed?.Invoke(profileId, "Profile not found");
                return;
            }

            var latest = profile.autoSave;
            foreach (var meta in profile.manualSaves)
            {
                if (string.IsNullOrEmpty(latest?.slotId) || meta.updatedUtcTicks > latest.updatedUtcTicks)
                    latest = meta;
            }

            if (string.IsNullOrEmpty(latest?.slotId))
            {
                GameLog.Warning(TAG, $"ContinueProfile: profile '{profileId}' has no saves yet.");
                LoadFailed?.Invoke(profileId, "No saves in this profile");
                return;
            }

            LoadSlot(profileId, latest.slotId);
        }

        /// <summary>
        /// Loads whichever save, in whichever profile, was updated most recently overall.
        /// Intended for a single "Continue" button on the main menu (no profile picked yet).
        /// </summary>
        public void ContinueLatestGame()
        {
            var profiles = ListProfiles();
            SaveProfile latestProfile = null;

            foreach (var profile in profiles)
            {
                bool hasAnySave = !string.IsNullOrEmpty(profile.autoSave?.slotId) || profile.manualSaves.Count > 0;
                if (!hasAnySave)
                    continue;

                if (latestProfile == null || profile.updatedUtcTicks > latestProfile.updatedUtcTicks)
                    latestProfile = profile;
            }

            if (latestProfile == null)
            {
                GameLog.Warning(TAG, "ContinueLatestGame: no profile has any saves yet.");
                LoadFailed?.Invoke(null, "No saves found");
                return;
            }

            ContinueProfile(latestProfile.profileId);
        }

        /// <summary>
        /// Wipes every registered ISaveable back to its default state (clears anything left over
        /// on persistent/DontDestroyOnLoad saveables from a previous run) and starts a fresh game
        /// in the active profile. Call CreateProfile first.
        /// </summary>
        public void NewGame(string sceneName)
        {
            if (!EnsureActiveProfile()) return;
            SaveRegistry.ResetAllToDefaults();
            GameFlowApi.StartGame(sceneName);
        }

        // ---------- Internals ----------

        private bool EnsureActiveProfile()
        {
            if (!string.IsNullOrEmpty(ActiveProfileId))
                return true;

            GameLog.Error(TAG, "No active profile - call CreateProfile, LoadSlot or ContinueProfile first.");
            return false;
        }

        private void HandleContentLoaded(string sceneName)
        {
            if (pendingLoadData == null || sceneName != pendingLoadData.sceneName)
                return; // not a scene load we initiated (e.g. GameFlowController starting a new game)

            var data = pendingLoadData;
            var slotId = pendingLoadSlotId;
            pendingLoadData = null;
            pendingLoadSlotId = null;

            SaveRegistry.RestoreAll(data.objectStates);

            IsBusy = false;
            GameStateManager.Instance.SetMode(GameMode.Gameplay);
            GameStateManager.Instance.SetPauseReason(TAG, false);

            LoadCompleted?.Invoke(slotId);
            GameLog.Log(TAG, $"LoadSlot('{slotId}') complete, scene='{data.sceneName}'");
        }

        private void SaveInternal(string slotId, string displayName, bool isAuto)
        {
            var data = new SaveSlotData
            {
                slotId = slotId,
                version = Application.version,
                sceneName = SceneLoader.Instance.CurrentContentScene,
                objectStates = SaveRegistry.CaptureAll()
            };

            try
            {
                SaveFileIO.WriteSlot(SlotPath(ActiveProfileId, slotId), data);

                var nowTicks = DateTime.UtcNow.Ticks;
                if (isAuto)
                {
                    ActiveProfile.autoSave ??= new SaveSlotMeta { slotId = slotId, createdUtcTicks = nowTicks };
                    ActiveProfile.autoSave.slotId = slotId;
                    ActiveProfile.autoSave.displayName = displayName;
                    ActiveProfile.autoSave.updatedUtcTicks = nowTicks;
                }
                else
                {
                    var meta = ActiveProfile.manualSaves.FirstOrDefault(m => m.slotId == slotId);
                    if (meta == null)
                    {
                        meta = new SaveSlotMeta { slotId = slotId, createdUtcTicks = nowTicks };
                        ActiveProfile.manualSaves.Add(meta);
                    }
                    if (!string.IsNullOrEmpty(displayName))
                        meta.displayName = displayName;
                    meta.updatedUtcTicks = nowTicks;
                }

                ActiveProfile.updatedUtcTicks = nowTicks; // so ListProfiles can show "last played" without opening slots
                SaveFileIO.WriteIndex(ProfileIndexPath(ActiveProfileId), ActiveProfile);
            }
            catch (Exception ex)
            {
                GameLog.Error(TAG, $"Save to slot '{slotId}' (profile '{ActiveProfileId}') failed: {ex.Message}");
                SaveFailed?.Invoke(slotId, ex.Message);
                return;
            }

            SaveCompleted?.Invoke(slotId);
            GameLog.Log(TAG, $"Saved slot '{slotId}' in profile '{ActiveProfileId}' (scene='{data.sceneName}')");
        }
    }
}