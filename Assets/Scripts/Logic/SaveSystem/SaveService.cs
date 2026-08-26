using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SaveSystem {
    [DefaultExecutionOrder(-1500)]
    public class SaveService : MonoBehaviour {
        public static SaveService Instance { get; private set; }

        [SerializeField] SaveConfig config;
        private SaveRepository repository = new SaveRepository();

        public SaveProfile ActiveProfile { get; private set; }
        public SaveSlotData PendingLoadData { get; private set; } // save data loaded to memory but hasn't been applied yet

        public bool CanResume() => repository.ListProfiles().Any(profile => profile.HasAnySave);
        
        public event Action ProfilesChanged;
        public event Action<string> SaveCompleted;        // slotId
        public event Action<string, string> SaveFailed;   // slotId, reason
        public event Action<string> LoadCompleted;        // slotId
        public event Action<string, string> LoadFailed;   // profileId or slotId, reason

        const string TAG = "SaveService";

        private void Awake() {
            if (Instance != null && Instance != this) { 
                Destroy(gameObject);
                return;
            }

            Instance = this;
            repository.EnsureFolder();
            GameLog.Log(TAG, "Initialized");
        }

        /// ================================
        /// ========== PUBLIC API ==========
        /// ================================

        ///  --- LOADING SAVE METHODS ----
        
        /// <summary>
        /// Prepares save service for new game being created.
        /// </summary>
        public void StartNewGame(string displayName) {
            SaveProfile newProfile = repository.CreateProfile(displayName, config).profile;

            ActiveProfile = newProfile;
            PendingLoadData = null;
        }

        /// <summary>
        /// Prepares save service for loading new game.
        /// </summary>
        public string StartExistingGame(string profileId) {
            string latestSlotId = repository.GetLatestSlotId(profileId).latestSlotId;
            var data = repository.GetData(profileId, latestSlotId);

            if (data == null) {
                LoadFailed?.Invoke(null, "Invalid data");
                return null;
            }

            PreparePendingData(profileId, latestSlotId);
            return data.sceneName;
        }

        /// <summary>
        /// Finds latest saveSlot out of all profiles and prepares to load it.
        /// </summary>
        public string Resume() {
            (string latestProfileId, string latestSlotId, string message) = repository.GetLatestSaveInfo();

            if (latestProfileId == null || latestSlotId == null) {
                LoadFailed?.Invoke(null, "Invalid data");
                return null;
            }

            var data = repository.GetData(latestProfileId, latestSlotId);

            if (data == null) {
                LoadFailed?.Invoke(null, "Invalid data");
                return null;
            }

            PreparePendingData(latestProfileId, latestSlotId);
            return data.sceneName;
        }

        /// <summary>
        /// Sets pending data (profile and slot id`s that will be applied when scene is loaded).
        /// </summary>
        public void PreparePendingData(string profileId, string slotId) {
            if (PendingLoadData != null) { 
                GameLog.Warning(TAG, "LoadSlot ignored: SaveManager is busy");
                return;
            }

            SaveProfile profile = repository.GetProfile(profileId);

            if (profile == null) {
                GameLog.Error(TAG, $"LoadSlot('{profileId}'/'{slotId}') failed: profile file missing or unreadable");
                LoadFailed?.Invoke(slotId, "Profile file missing or unreadable");
                return;
            }

            SaveSlotData slotData = repository.GetData(profileId, slotId);

            if (slotData == null) {
                GameLog.Error(TAG, $"LoadSlot('{profileId}'/'{slotId}') failed: slot file missing or unreadable");
                LoadFailed?.Invoke(slotId, "Slot file missing or unreadable");
                return;
            }

            ActiveProfile = profile;
            PendingLoadData = slotData;
        }

        /// <summary>
        /// When scene is loaded applies PendingLoadData
        /// </summary>
        public void ApplyPendingData() {
            SaveRegistry.ResetAllToDefaults();

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
        /// </summary> 
        public void AutoSave(string sceneName) => SaveToActiveProfile("autosave", "Auto Save", sceneName,  isAutoSave: true);

        /// <summary>
        /// Create a new manual save slot in active profile.  
        /// Then save current game state to created slot in active profile. Return new slotId or null if failed. 
        /// </summary>
        public void NewManualSave(string displayName, string sceneName) => SaveToActiveProfile(Guid.NewGuid().ToString("N"), displayName, sceneName, isAutoSave: false);

        /// <summary>
        /// Overwrites manual save in active profile.
        /// </summary>
        public void OverwriteManual(string slotId, string displayName, string sceneName) => SaveToActiveProfile(slotId, displayName, sceneName, isAutoSave: false);
        
        /// <summary>
        /// 
        /// </summary>
        private void SaveToActiveProfile(string slotId, string displayName, string sceneName, bool isAutoSave) {
            if (ActiveProfile == null) {
                SaveFailed?.Invoke(null, "No Active profile set");
                return;
            }

            if (string.IsNullOrEmpty(slotId)) {
                SaveFailed?.Invoke(null, "Invalid slotId");
                return;
            }
            
            var data = new SaveSlotData {
                slotId = slotId,
                version = Application.version,
                sceneName = sceneName,
                objectStates = SaveRegistry.CaptureAll()
            };

            (SaveProfile updatedProfile, string message) = repository.SaveData(ActiveProfile.id, data, displayName, isAutoSave, config);

            if (updatedProfile != null) {
                ActiveProfile = updatedProfile;
                Debug.Log("Saved Successfully !");
                SaveCompleted?.Invoke(slotId);
            } else {
                Debug.Log("Failed to Save: " + message);
                SaveFailed?.Invoke(slotId, message);
            }
                
        }      

        /// <summary>
        /// Deletes a sprcified manual save from active profile
        /// </summary>
        public void DeleteManualSave(string slotId) {
            var newProfile =  repository.DeleteData(ActiveProfile.id, slotId);

            if (newProfile == null) 
                return;

             ActiveProfile = newProfile;
        }

        public List<SaveProfile> GetAllProfiles() {
            return repository.ListProfiles();
        }
    }
}