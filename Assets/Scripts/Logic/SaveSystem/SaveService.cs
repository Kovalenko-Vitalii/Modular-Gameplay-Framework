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

        public SaveProfile ActiveProfile { get; private set; }
        public SaveSlotData PendingLoadData { get; private set; } // save data loaded to memory but hasn't been applied yet
        public Action SlotsChanged { get; internal set; }

        /// <summary>True when any profile contains at least one save slot.</summary>
        public bool CanResume() => Saves.GetAllProfiles().Any(profile => profile.HasAnySave);

        /// <summary>Raised when profiles or their metadata change (UI can refresh).</summary>
        public event Action ProfilesChanged;

        /// <summary>Initialize singleton instance and ensure repository folder exists.</summary>
        private void Awake() {
            if (Instance != null && Instance != this) { 
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Saves.EnsureFolder();
            Debug.Log("Initialized");
        }

        /// ================================
        /// ========== PUBLIC API ==========
        /// ================================

        ///  --- LOADING SAVE METHODS ----
        
        /// <summary>
        /// Create a new profile for a fresh game, set it active and notify listeners.
        /// </summary> v
        public void StartNewGame(string displayName) {
            if (!H.ValidateString(displayName)) return;

            var response = Saves.CreateProfile(displayName, config);

            if (!H.ValidateProfile(response.profile)) { Debug.Log($"Couldn`t create profile for new game: '{response.message}' "); return; }
            
            ActiveProfile = response.profile;
            ProfilesChanged?.Invoke();
            PendingLoadData = null;
        }

        /// <summary>
        /// Prepare to load the most recent slot for the given profile. Returns the
        /// scene name to load or null on failure.
        /// </summary> v
        public string StartLatestFrom(string profileId) {
            if (!H.ValidateString(profileId)) return null; 
                
            string latestSlotId = Saves.GetLatestMeta(profileId);
            
            if (!H.ValidateString(latestSlotId)) return null;

            var data = Saves.GetData(profileId, latestSlotId);

            if (!H.ValidateData(data)) return null;
                
            PreparePendingData(profileId, latestSlotId);
            return data.sceneName;
        }

        /// <summary>
        /// Locate the latest save across all profiles and stage it for loading.
        /// Returns the scene name to load or null on failure.
        /// </summary> v
        public string StartLatestGlobal() {
            (string latestProfileId, string latestSlotId) = Saves.GetLatestProfile();

            if (!H.ValidateString(latestProfileId)) return null; 
            if (!H.ValidateString(latestSlotId)) return null;

            var data = Saves.GetData(latestProfileId, latestSlotId);

            if (!H.ValidateData(data)) return null; 

            PreparePendingData(latestProfileId, latestSlotId);
            return data.sceneName;
        }

        /// <summary>
        /// Prepate to load any save. 
        /// Returns the scene name to load or null on failure.
        /// </summary> v
        public string StartFrom(string profileId, string slotId) {
            if (!H.ValidateString(profileId)) return null; 
            if (!H.ValidateString(slotId)) return null; 

            var data = Saves.GetData(profileId, slotId);

            if (!H.ValidateData(data)) return null; 

            PreparePendingData(profileId, slotId);
            return data.sceneName;
        }

        /// <summary>
        /// Load and validate the profile and slot files then store them in PendingLoadData.
        /// Does not apply data to the scene; caller must call ApplyPendingData afterwards.
        /// </summary>
        public void PreparePendingData(string profileId, string slotId) {
            if (PendingLoadData != null) { Debug.Log("LoadSlot ignored: SaveManager is busy"); return; }
            if (!H.ValidateString(profileId) || !H.ValidateString(slotId)) return;

            SaveProfile profile = Saves.GetProfile(profileId);

            if (!H.ValidateProfile(profile)) return;
               
            SaveSlotData slotData = Saves.GetData(profileId, slotId);

            if (!H.ValidateData(slotData)) return;

            ActiveProfile = profile;
            PendingLoadData = slotData;
        }

        /// <summary>
        /// Apply staged save data to the current scene: reset registry then restore object states.
        /// </summary>
        public void ApplyPendingData() {
            SaveRegistry.ResetAllToDefaults(); // in case new game started.

            if (!H.ValidateData(PendingLoadData)) return;
                
            var data = PendingLoadData;
            PendingLoadData = null;

            SaveRegistry.RestoreAll(data.objectStates);

            Debug.Log($"LoadSlot('{data.id}') complete'");
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
            if (PendingLoadData != null) { Debug.Log("Save ignored: a load is pending"); return; }
            if (!H.ValidateProfile(ActiveProfile)) return;
            if (!H.ValidateString(slotId)) return;
                
            var data = new SaveSlotData {
                id = slotId,
                version = Application.version,
                sceneName = sceneName,
                objectStates = SaveRegistry.CaptureAll()
            };

            SaveProfile updatedProfile = Saves.SaveData(ActiveProfile.id, data, displayName, isAutoSave, config);

            if (H.ValidateProfile(updatedProfile)) {
                ActiveProfile = updatedProfile;
                ProfilesChanged?.Invoke();
                Debug.Log("Saved Successfully !");
            } else 
                Debug.Log("Failed to Save");    
        }      

        /// <summary>
        /// Remove a manual save slot from the active profile and update ActiveProfile when successful.
        /// </summary>
        public void DeleteManualSave(string slotId) {
            if (!H.ValidateProfile(ActiveProfile)) return;
            if (!H.ValidateString(slotId)) return;

            var newProfile = Saves.DeleteData(ActiveProfile.id, slotId);

            if (!H.ValidateProfile(newProfile)) return;

             ActiveProfile = newProfile;
             ProfilesChanged?.Invoke();
        }

        /// <summary>
        /// Delete the profile and its files from disk. Logs the outcome.
        /// </summary>
        public void DeleteProfile(string profileId) {
            if (!H.ValidateString(profileId)) return;

            bool deleted = Saves.DeleteProfile(profileId);

            if (deleted) {
                if (ActiveProfile?.id == profileId) ActiveProfile = null;
                ProfilesChanged?.Invoke();
                Debug.Log($"Profile with id: '{profileId}' is deleted");
            }
            else Debug.Log($"Couldn`t delete rofile with id: '{profileId}'");
               
        }

        /// <summary>
        /// Return all profiles known to the repository (cached on disk).
        /// </summary>
        public List<SaveProfile> GetAllProfiles() {
            return Saves.GetAllProfiles();
        }

        public List<SaveSlotMeta> GetAllSlotsFromActive() {
            if (!H.ValidateProfile(ActiveProfile)) return new List<SaveSlotMeta>();
            return Saves.GetAllMeta(ActiveProfile.id);
        }

    }
}