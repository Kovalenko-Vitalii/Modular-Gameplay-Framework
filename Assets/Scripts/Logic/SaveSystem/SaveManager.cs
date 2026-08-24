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
        public static SaveManager Instance { get; private set; }

        public SaveProfile ActiveProfile { get; private set; }
        public SaveSlotData PendingLoadData { get; private set; } // save data loaded to memory but hasn't been applied yet

        public bool HasAutoSave => !string.IsNullOrEmpty(ActiveProfile?.autoSave?.slotId);
        public bool CanContinue() => ListProfiles().Any(profile => !string.IsNullOrEmpty(profile.autoSave?.slotId) || profile.manualSaves.Count > 0);

        private string SavesFolder => Path.Combine(Application.persistentDataPath, "Saves");
        private string ProfileFolder(string profileId) => Path.Combine(SavesFolder, profileId);
        private string ProfileIndexPath(string profileId) => Path.Combine(ProfileFolder(profileId), "index.json");
        private string ProfileSlotsFolder(string profileId) => Path.Combine(ProfileFolder(profileId), "Slots");
        private string SlotPath(string profileId, string slotId) => Path.Combine(ProfileSlotsFolder(profileId), slotId + ".json");
        
        public event Action ProfilesChanged;
        public event Action<string> SaveCompleted;        // slotId
        public event Action<string, string> SaveFailed;   // slotId, reason
        public event Action<string> LoadCompleted;        // slotId
        public event Action<string, string> LoadFailed;   // profileId or slotId, reason

        const string TAG = "SaveManager";

        private void Awake() {
            if (Instance != null && Instance != this) { 
                Destroy(gameObject);
                return;
            }

            Instance = this;
            SaveFileIO.EnsureFolder(SavesFolder);
            GameLog.Log(TAG, "Initialized");
        }

        /// ================================
        /// ========== PUBLIC API ==========
        /// ================================

        ///  --- LOADING SAVE METHODS ----
        
        /// <summary>
        /// Takes profile and slot id`s that will be applied when scene is loaded.
        /// </summary>
        public void CacheData(string profileId, string slotId) {
            if (PendingLoadData != null) { 
                GameLog.Warning(TAG, "LoadSlot ignored: SaveManager is busy");
                return;
            }

            var data = SaveFileIO.GetSlotData(SlotPath(profileId, slotId));

            if (data == null) {
                GameLog.Error(TAG, $"LoadSlot('{profileId}'/'{slotId}') failed: slot file missing or unreadable");
                LoadFailed?.Invoke(slotId, "Slot file missing or unreadable");
                return;
            }

            ActiveProfile = SaveFileIO.GetProfile(ProfileIndexPath(profileId));
            ActiveProfile.profileId = profileId;

            PendingLoadData = data;
        }

        /// <summary>
        /// When scene is loaded apply PendingLoadData
        /// </summary>
        public void ApplyCacheData() {
            SaveRegistry.ResetAllToDefaults(); // in case it is new game with nothing cached

            if (PendingLoadData == null)
                return;

            var data = PendingLoadData;
            PendingLoadData = null;

            SaveRegistry.RestoreAll(data.objectStates);

            LoadCompleted?.Invoke(data.slotId);
            GameLog.Log(TAG, $"LoadSlot('{data.slotId}') complete, scene='{data.sceneName}'");
            return;
        }

        /// --- SAVE OPERATIONS ---

        /// <summary>
        /// Saves current game state to a auto-save slot in active profile.
        /// </summary> !!!
        public void AutoSave(string profileId) => SaveInternal(profileId, "autosave", "Auto Save", isAutoSave: true);

        /// <summary>
        /// Create a new manual save slot in active profile.  
        /// Then save current game state to created slot in active profile. Return new slotId or null if failed. 
        /// </summary>
        public string NewManualSave(string profileId, string displayName) {
            var slotId = Guid.NewGuid().ToString("N");
            SaveInternal(profileId, slotId, displayName, isAutoSave: false);
            return slotId;
        }

        /// <summary>
        /// Overwrites manual save in active profile.
        /// </summary> !!!
        public void OverwriteManual(string profileId, string slotId) => SaveInternal(profileId, slotId, null, isAutoSave: false);        

        /// <summary>
        /// Deletes a manual save from any profile, active or not.
        /// </summary>
        public void DeleteManualSave(string profileId, string slotId) {
            var profilePath = ProfileIndexPath(profileId);
            var profile = SaveFileIO.GetProfile(profilePath);
            var meta = profile.manualSaves.FirstOrDefault(m => m.slotId == slotId);

            if (meta == null)
                return;

            SaveFileIO.DeleteSlot(SlotPath(profileId, slotId));
            profile.manualSaves.Remove(meta);
            SaveFileIO.WriteProfile(profilePath, profile);

            if (ActiveProfile.profileId == profileId)
                ActiveProfile = profile;
        }

        /// --- PROFILE OPERATIONS ---
        
        /// <summary>
        /// Returns all valid save profiles found on disk.
        /// </summary>
        public List<SaveProfile> ListProfiles() {
            SaveFileIO.EnsureFolder(SavesFolder);
            var result = new List<SaveProfile>();

            foreach (var dir in Directory.GetDirectories(SavesFolder)) {
                var profileId = Path.GetFileName(dir);
                var indexPath = ProfileIndexPath(profileId);
                if (!File.Exists(indexPath))
                    continue;

                var profile = SaveFileIO.GetProfile(indexPath);
                if (string.IsNullOrEmpty(profile.profileId)) {
                    GameLog.Warning(TAG, $"Skipping unreadable/corrupt profile folder '{profileId}'.");
                    continue;
                }

                result.Add(profile);
            }

            return result;
        }

        /// <summary>
        /// Creates and activates a new save profile.
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

            SaveFileIO.EnsureFolder(ProfileFolder(profileId));
            SaveFileIO.WriteProfile(ProfileIndexPath(profileId), profile);

            ActiveProfile = profile;

            GameLog.Log(TAG, $"Created profile '{profileId}' ('{displayName}')");
            return profile;
        }

        /// <summary>
        /// Deletes profile with all related data.
        /// </summary>
        public void DeleteProfile(string profileId) {
            var dir = ProfileFolder(profileId);

            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);

            if (ActiveProfile.profileId == profileId) 
                ActiveProfile = null;
            

            GameLog.Log(TAG, $"Deleted profile '{profileId}'");
        }

        /// <summary>
        /// Updates display name of existing profile. 
        /// Does not change active profile unless the renamed profile is currently active.
        /// </summary>
        public void RenameProfile(string profileId, string displayName) {
            if (profileId == ActiveProfile.profileId) {
                ActiveProfile.displayName = displayName;
                ActiveProfile.updatedUtcTicks = DateTime.UtcNow.Ticks;
                return;
            }

            var path = ProfileIndexPath(profileId);
            var profile = SaveFileIO.GetProfile(path);
            if (string.IsNullOrEmpty(profile.profileId)) {
                GameLog.Warning(TAG, $"RenameProfile: profile '{profileId}' not found.");
                return;
            }

            profile.displayName = displayName;
            profile.updatedUtcTicks = DateTime.UtcNow.Ticks;
            SaveFileIO.WriteProfile(path, profile);

            if (ActiveProfile.profileId == profileId)
                ActiveProfile = profile;
        }

        /// --- GETTERS ---
        public SaveSlotData GetSaveData(string profileId, string slotId) {
            return SaveFileIO.GetSlotData(SlotPath(profileId, slotId));
        }

        /// <summary>
        /// Returns information about 
        /// </summary>
        public (string, string) GetLatestSaveInfo() {
            SaveProfile latestProfile = null;

            foreach (var profile in ListProfiles()) {
                if (!profile.HasAnySave)
                    continue;

                if (latestProfile == null || profile.updatedUtcTicks > latestProfile.updatedUtcTicks)
                    latestProfile = profile;
            }

            if (latestProfile == null) {
                GameLog.Warning(TAG, "ContinueLatestGame: no profile has any saves yet.");
                LoadFailed?.Invoke(null, "No saves found");
                return (null, null);
            }

            string latestSlotId = GetLatestSlotId(latestProfile.profileId);
            
            return (latestProfile.profileId , latestSlotId);
        }

        /// <summary>
        /// 
        /// </summary>
        public string GetLatestSlotId(string profileId) {
            var profile = SaveFileIO.GetProfile(ProfileIndexPath(profileId));

            if (string.IsNullOrEmpty(profile.profileId)) {
                GameLog.Error(TAG, $"ContinueProfile: profile '{profileId}' not found.");
                LoadFailed?.Invoke(profileId, "Profile not found");
                return null;
            }

            SaveSlotMeta latest = profile.Latest();

            if (string.IsNullOrEmpty(latest?.slotId)) {
                GameLog.Warning(TAG, $"ContinueProfile: profile '{profileId}' has no saves yet.");
                LoadFailed?.Invoke(profileId, "No saves in this profile");
                return null;
            }

            return latest.slotId;
        }


        /// -----------------------------
        /// ---------- PRIVATE ----------
        /// -----------------------------

        /// <summary>
        /// 
        /// </summary>
        private void SaveInternal(string profileId, string slotId, string slotDisplayName, bool isAutoSave) {
            var data = new SaveSlotData { 
                slotId = slotId,
                version = Application.version,
                sceneName = SceneLoader.Instance.CurrentContentScene,
                objectStates = SaveRegistry.CaptureAll()
            };

            try {

                var profile = ResolveProfileForWrite(profileId);
                if (profile == null)
                {
                    SaveFailed?.Invoke(slotId, "Profile not found");
                    return;
                }

                SaveFileIO.EnsureFolder(ProfileSlotsFolder(profileId));
                SaveFileIO.WriteSlotData(SlotPath(profileId, slotId), data);

                var nowTicks = DateTime.UtcNow.Ticks;

                if (isAutoSave) {
                    profile.autoSave ??= new SaveSlotMeta { 
                        slotId = slotId, 
                        createdUtcTicks = nowTicks 
                    };
                    profile.autoSave.slotId = slotId;
                    profile.autoSave.displayName = slotDisplayName;
                    profile.autoSave.updatedUtcTicks = nowTicks;
                }
                else {
                    var meta = profile.manualSaves.FirstOrDefault(m => m.slotId == slotId);
                    if (meta == null) {
                        meta = new SaveSlotMeta { slotId = slotId, createdUtcTicks = nowTicks };
                        profile.manualSaves.Add(meta);
                    }
                    if (!string.IsNullOrEmpty(slotDisplayName))
                        meta.displayName = slotDisplayName;
                    meta.updatedUtcTicks = nowTicks;
                }

                profile.updatedUtcTicks = nowTicks;
                SaveFileIO.WriteProfile(ProfileIndexPath(profileId), profile);
            }
            catch (Exception ex) {
                GameLog.Error(TAG, $"Save to slot '{slotId}' (profile '{profileId}') failed: {ex.Message}");
                SaveFailed?.Invoke(slotId, ex.Message);
                return;
            }

            SaveCompleted?.Invoke(slotId);
            GameLog.Log(TAG, $"Saved slot '{slotId}' in profile '{profileId}' (scene='{data.sceneName}')");
        }

        private SaveProfile ResolveProfileForWrite(string profileId) {
            if (ActiveProfile != null && profileId == ActiveProfile.profileId)
                return ActiveProfile;

            var profile = SaveFileIO.GetProfile(ProfileIndexPath(profileId));

            if (string.IsNullOrEmpty(profile?.profileId)) {
                GameLog.Error(TAG, $"ResolveProfileForWrite: profile '{profileId}' not found on disk and is not the pending active profile.");
                return null;
            }

            return profile;
        }
    }
}