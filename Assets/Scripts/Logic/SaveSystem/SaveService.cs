using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SaveSystem {
    /// <summary>
    /// Singleton service coordinating save/load operations.
    /// Orchestrates repository access, maintains the active profile, and stages
    /// loaded slot data in PendingLoadData for later application.
    /// </summary>
    [DefaultExecutionOrder(-1500)]
    public class SaveService : MonoBehaviour {
        public static SaveService Instance { get; private set; }

        [SerializeField] SaveConfig config;
        private SaveRepository repository = new SaveRepository();

        public SaveProfile ActiveProfile { get; private set; }
        public SaveSlotData PendingLoadData { get; private set; } // save data loaded to memory but hasn't been applied yet

        /// <summary>True when any profile contains at least one save slot.</summary>
        public bool CanResume() => repository.ListProfiles().Any(profile => profile.HasAnySave);

        /// <summary>Raised when profiles or their metadata change (UI can refresh).</summary>
        public event Action ProfilesChanged;

        /// <summary>Initialize singleton instance and ensure repository folder exists.</summary>
        private void Awake() {
            if (Instance != null && Instance != this) { 
                Destroy(gameObject);
                return;
            }

            Instance = this;
            repository.EnsureFolder();
            Debug.Log("Initialized");
        }

        /// ================================
        /// ========== PUBLIC API ==========
        /// ================================

        ///  --- LOADING SAVE METHODS ----
        
        /// <summary>
        /// Create a new profile for a fresh game, set it active and notify listeners.
        /// </summary>
        public void StartNewGame(string displayName) {
            var response = repository.CreateProfile(displayName, config);

            if (response.profile == null) {
                Debug.Log($"Couldn`t create profile for new game: '{response.message}' ");
                return;
            }
            
            ActiveProfile = response.profile;
            ProfilesChanged?.Invoke();
            PendingLoadData = null;
        }

        /// <summary>
        /// Prepare to load the most recent slot for the given profile. Returns the
        /// scene name to load or null on failure.
        /// </summary>
        public string StartExistingGame(string profileId) {
            string latestSlotId = repository.GetLatestSlotId(profileId).latestSlotId;
            var data = repository.GetData(profileId, latestSlotId);

            if (data == null || string.IsNullOrEmpty(data.slotId)) {
                Debug.Log("Invalid data");
                return null;
            }

            PreparePendingData(profileId, latestSlotId);
            return data.sceneName;
        }

        /// <summary>
        /// Locate the latest save across all profiles and stage it for loading.
        /// Returns the scene name to load or null on failure.
        /// </summary>
        public string Resume() {
            (string latestProfileId, string latestSlotId, string message) = repository.GetLatestSaveInfo();

            if (latestProfileId == null || latestSlotId == null) {
                Debug.Log("Invalid data");
                return null;
            }

            var data = repository.GetData(latestProfileId, latestSlotId);

            if (data == null) {
                Debug.Log("Invalid data");
                return null;
            }

            PreparePendingData(latestProfileId, latestSlotId);
            return data.sceneName;
        }

        /// <summary>
        /// Load and validate the profile and slot files then store them in PendingLoadData.
        /// Does not apply data to the scene; caller must call ApplyPendingData afterwards.
        /// </summary>
        public void PreparePendingData(string profileId, string slotId) {
            if (PendingLoadData != null) {
                Debug.Log("LoadSlot ignored: SaveManager is busy");
                return;
            }

            SaveProfile profile = repository.GetProfile(profileId);

            if (profile == null) {
                Debug.Log($"LoadSlot('{profileId}'/'{slotId}') failed: profile file missing or unreadable");
                return;
            }

            SaveSlotData slotData = repository.GetData(profileId, slotId);

            if (slotData == null) {
                Debug.Log($"LoadSlot('{profileId}'/'{slotId}') failed: slot file missing or unreadable");
                return;
            }

            ActiveProfile = profile;
            PendingLoadData = slotData;
        }

        /// <summary>
        /// Apply staged save data to the current scene: reset registry then restore object states.
        /// </summary>
        public void ApplyPendingData() {
            SaveRegistry.ResetAllToDefaults(); // in case new game started.

            if (PendingLoadData == null) 
                return;    
                
            var data = PendingLoadData;
            PendingLoadData = null;

            SaveRegistry.RestoreAll(data.objectStates);

            Debug.Log($"LoadSlot('{data.slotId}') complete, scene='{data.sceneName}'");
            return;
        }

        /// --- SAVE OPERATIONS ---

        /// <summary>Save current game state to the active profile's auto-save slot.</summary>
        public void AutoSave(string sceneName) => SaveToActiveProfile("autosave", "Auto Save", sceneName,  isAutoSave: true);

        /// <summary>Create a new manual save slot and persist current state to it.</summary>
        public void NewManualSave(string displayName, string sceneName) => SaveToActiveProfile(Guid.NewGuid().ToString("N"), displayName, sceneName, isAutoSave: false);

        /// <summary>Overwrite an existing manual save slot with the current state.</summary>
        public void OverwriteManual(string slotId, string displayName, string sceneName) => SaveToActiveProfile(slotId, displayName, sceneName, isAutoSave: false);
        
        /// <summary>
        /// Capture current object states and persist them via the repository. On success
        /// ActiveProfile is updated; on failure a message is logged.
        /// </summary>
        private void SaveToActiveProfile(string slotId, string displayName, string sceneName, bool isAutoSave) {
            if (ActiveProfile == null) {
                Debug.Log("No Active profile set");
                return;
            }

            if (string.IsNullOrEmpty(slotId)) {
                Debug.Log("Invalid slotId");
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
            } else 
                Debug.Log("Failed to Save: " + message);    
        }      

        /// <summary>
        /// Remove a manual save slot from the active profile and update ActiveProfile when successful.
        /// </summary>
        public void DeleteManualSave(string slotId) {
            if (ActiveProfile == null || string.IsNullOrEmpty(ActiveProfile.id))
                return;

            var newProfile =  repository.DeleteData(ActiveProfile.id, slotId);

            if (newProfile == null) 
                return;

             ActiveProfile = newProfile;
        }

        /// <summary>Delete the profile and its files from disk. Logs the outcome.</summary>
        public void DeleteProfile(string profileId) {
            bool response = repository.DeleteProfile(profileId);
            if (response)
                Debug.Log($"Profile with id: '{profileId}' is deleted");
            else
                Debug.Log($"Couldn`t delete rofile with id: '{profileId}'");
        }

        /// <summary>Return all profiles known to the repository (cached on disk).</summary>
        public List<SaveProfile> GetAllProfiles() {
            return repository.ListProfiles();
        }
    }
}