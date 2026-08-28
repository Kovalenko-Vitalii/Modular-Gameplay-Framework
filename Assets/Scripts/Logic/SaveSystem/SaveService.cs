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
        public SaveSlotData PendingLoadData { get; private set; }

        public event Action ProfilesChanged;

        private void Awake() {
            if (Instance != null && Instance != this) { 
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Saves.EnsureFolder();
            Debug.Log("Initialized");
        }

        #region Load Operations
        /// <summary> Prepares SaveService to load new game. </summary>
        public void StartNewGame(string displayName) {
            if (string.IsNullOrEmpty(displayName)) return; 

            var newProfile = Saves.CreateProfile(displayName, config);
            if (!newProfile.IsValid()) return; 
            
            ActiveProfile = newProfile;
            ProfilesChanged?.Invoke();
            PendingLoadData = null;
        }

        /// <summary> Prepares SaveService to load latest save from selected profile.
        /// <returns> Scene name of latest save. </returns>
        public string StartLatestFrom(string profileId) {
            if (string.IsNullOrEmpty(profileId)) return null; 
                
            var latestMeta = Saves.GetLatestMeta(profileId);
            if (!latestMeta.IsValid() || string.IsNullOrEmpty(latestMeta.sceneName)) return null;
                
            PreparePendingData(profileId, latestMeta.id);
            return latestMeta.sceneName;
        }

        /// <summary> Prepares SaveService to load latest save from all profiles. </summary> 
        /// <returns> Scene name of latest save. </returns> 
        public string StartLatestGlobal() {
            SaveProfile latestProfile = Saves.GetLatestProfile();
            if (!latestProfile.IsValid()) return null;

            SaveSlotMeta latestMeta = Saves.GetLatestMeta(latestProfile.id);
            if (!latestMeta.IsValid()) return null;

            PreparePendingData(latestProfile.id, latestMeta.id);
            return latestMeta.sceneName;
        }

        /// <summary> Prepares SaveService to load any save. </summary>
        /// <returns> Scene name of latest save. </returns>  
        public string StartFrom(string profileId, string saveId) {
            if (string.IsNullOrEmpty(profileId) || string.IsNullOrEmpty(saveId)) return null;

            var profile = Saves.GetProfile(profileId);
            if (!profile.IsValid()) return null;
 
            var data = Saves.GetData(profileId, saveId);
            if (!data.IsValid()) return null; 

            PreparePendingData(profileId, saveId);
            return data.sceneName;
        }

        /// Write load options here =>

        /// <summary>
        /// Prepares SaveService to load specified save data in specified profile.
        /// Does not apply data to the scene; caller must call ApplyPendingData afterwards.
        /// </summary>
        private void PreparePendingData(string profileId, string saveId) {
            if (PendingLoadData != null) { Debug.Log("LoadSlot ignored: SaveManager is busy"); return; }
            if (string.IsNullOrEmpty(profileId) || string.IsNullOrEmpty(saveId)) return;

            SaveProfile profile = Saves.GetProfile(profileId);
            if (profile.IsValid()) return;
               
            SaveSlotData slotData = Saves.GetData(profileId, saveId);
            if (slotData.IsValid()) return;

            ActiveProfile = profile;
            PendingLoadData = slotData;
        }

        /// <summary>
        /// Apply staged save data to the current scene: reset registry then restore object states.
        /// </summary>
        public void ApplyPendingData() {
            SaveRegistry.ResetAllToDefaults(); // in case new game started. !!!

            if (!PendingLoadData.IsValid()) return;
                
            var data = PendingLoadData;
            PendingLoadData = null;

            SaveRegistry.RestoreAll(data.objectStates);

            Debug.Log($"LoadSlot('{data.id}') complete'");
            return;
        }
        #endregion

        #region Save Operations
        /// <summary> Save current game state to the active profile's auto-save slot. </summary>
        public void AutoSave(string sceneName) => SaveToActiveProfile("autosave", "Auto Save", sceneName,  isAutoSave: true);

        /// <summary> Create a new manual save slot and persist current state to it.</summary>
        public void NewManualSave(string displayName, string sceneName) => SaveToActiveProfile(Guid.NewGuid().ToString("N"), displayName, sceneName, isAutoSave: false);

        /// <summary>Overwrite an existing manual save slot with the current state.</summary>
        public void OverwriteManual(string saveId, string displayName, string sceneName) => SaveToActiveProfile(saveId, displayName, sceneName, isAutoSave: false);
        
        /// <summary>
        /// Capture current object states and persist them via the repository. On success
        /// ActiveProfile is updated; on failure a message is logged.
        /// </summary>
        private void SaveToActiveProfile(string saveId, string displayName, string sceneName, bool isAutoSave) {
            if (PendingLoadData != null) { Debug.Log("Save ignored: a load is pending"); return; }
            if (!ActiveProfile.IsValid()) return;
            if (string.IsNullOrEmpty(saveId)) return;
                
            var data = new SaveSlotData {
                id = saveId,
                version = Application.version,
                sceneName = sceneName,
                objectStates = SaveRegistry.CaptureAll()
            };

            SaveProfile updatedProfile = Saves.SaveData(ActiveProfile.id, data, displayName, isAutoSave, config);

            if (updatedProfile.IsValid()) {
                ActiveProfile = updatedProfile;
                ProfilesChanged?.Invoke();
                Debug.Log("Saved Successfully !");
            } else 
                Debug.Log("Failed to Save");    
        }
        #endregion

        #region Other Operations
        /// <summary> Remove a manual save slot from the active profile and update ActiveProfile when successful. </summary>
        public void DeleteManualSave(string saveId) {
            if (!ActiveProfile.IsValid()) return;
            if (string.IsNullOrEmpty(saveId)) return;

            var newProfile = Saves.DeleteData(ActiveProfile.id, saveId);
            if (!newProfile.IsValid()) return;

             ActiveProfile = newProfile;
             ProfilesChanged?.Invoke();
        }

        /// <summary> Delete the profile and its files from disk. Logs the outcome. </summary>
        public void DeleteProfile(string profileId) {
            if (!string.IsNullOrEmpty(profileId)) return;

            Saves.DeleteProfile(profileId);

            /*
            if (deleted) {
                if (ActiveProfile?.id == profileId) ActiveProfile = null;
                ProfilesChanged?.Invoke();
                Debug.Log($"Profile with id: '{profileId}' is deleted");
            }
            else Debug.Log($"Couldn`t delete rofile with id: '{profileId}'");
            */
        }

        /// <summary> Return all profiles known to the repository (cached on disk). </summary>
        public List<SaveProfile> GetAllProfiles() {
            return Saves.GetAllProfiles();
        }

        public List<SaveSlotMeta> GetAllSlotsFromActive() {
            if (!ActiveProfile.IsValid()) return new List<SaveSlotMeta>();
            return Saves.GetAllMeta(ActiveProfile.id);
        }

        public bool CanResume() => Saves.GetAllProfiles().Any(profile => profile.HasAnySave);
        #endregion
    }
}