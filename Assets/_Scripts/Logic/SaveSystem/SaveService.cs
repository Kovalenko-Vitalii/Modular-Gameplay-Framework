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
    public class SaveService : MonoBehaviour, IService {
        public static SaveService Instance { get; private set; }

        [SerializeField] private SaveConfig config;

        public SaveProfile ActiveProfile { get; private set; }

        private SaveData PendingLoadData { get; set; }

        public event Action ProfilesChanged;

        public bool CanResume() => Saves.GetAllProfiles().Any(profile => profile.HasAnySave);

        public void Initialize() { }

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
            if (!latestMeta.IsValid() || string.IsNullOrEmpty(latestMeta.activeScene)) return null;

            PreparePendingData(profileId, latestMeta.id);
            return latestMeta.activeScene;
        }

        /// <summary> Prepares SaveService to load latest save from all profiles. </summary> 
        /// <returns> Scene name of latest save, null if not found. </returns> 
        public string StartLatestGlobal() {
            var latestProfile = Saves.GetLatestProfile();
            if (!latestProfile.IsValid()) return null;

            var latestMeta = Saves.GetLatestMeta(latestProfile.id);
            if (!latestMeta.IsValid()) return null;

            PreparePendingData(latestProfile.id, latestMeta.id);
            return latestMeta.activeScene;
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
            return data.activeScene;
        }

        /// Write load options here =>
        /// <summary>
        /// Prepares SaveService to load specified save data in specified profile.
        /// Does not apply data to the scene; caller must call ApplyPendingData afterward.
        /// </summary>
        private void PreparePendingData(string profileId, string saveId) {
            if (PendingLoadData != null) { Debug.Log("LoadSlot ignored: SaveService is busy"); return; }

            if (string.IsNullOrEmpty(profileId) || string.IsNullOrEmpty(saveId)) return;

            var profile = Saves.GetProfile(profileId);
            if (!profile.IsValid()) return;

            var slotData = Saves.GetData(profileId, saveId);
            if (!slotData.IsValid()) return;

            ActiveProfile = profile;
            PendingLoadData = slotData;
        }

        /// <summary>
        /// Apply staged save data to the current scene: reset registry then restore object states.
        /// </summary>
        public void ApplyPendingData(string sceneName) {
            if (!PendingLoadData.IsValid()) return;

            var data = PendingLoadData;
            PendingLoadData = null;

            var sceneData = data.GetSceneData(sceneName);
            if (sceneData == null) return;

            SaveRegistry.RestoreAll(sceneData.objectStates);

            Debug.Log($"LoadSlot('{data.id}') complete'");
            return;
        }

        public void Clean() {
            SaveRegistry.ResetAllToDefaults();
            ActiveProfile = null;
            PendingLoadData = null;
        }

        #endregion

        #region Save Operations

        /// <summary> Create a new manual save slot and persist current state to it.</summary>
        public void NewSave(string displayName, string sceneName) {
            if (!CanSave()) return;
            if (string.IsNullOrEmpty(displayName)) return; 
            if (string.IsNullOrEmpty(sceneName)) return; 

            var sceneData = new SceneData(sceneName, SaveRegistry.CaptureAll());
            var newData = new SaveData(Application.version, sceneName);

            var latestMeta = ActiveProfile.Latest();
            if (latestMeta.IsValid()) {
                var latestData = Saves.GetData(ActiveProfile.id, latestMeta.id);
                if (latestData.IsValid())
                    newData.scenes = latestData.scenes;
            }

            newData.AddSceneData(sceneData);
            UpdateProfile(newData, displayName, false);
        }

        /// <summary> Overwrite an existing manual save slot with the current state. </summary>
        public void OverwriteSave(string saveId, string sceneName, string saveName) {
            if (!CanSave()) return;
            if (string.IsNullOrEmpty(saveId)) return; 
            if (string.IsNullOrEmpty(sceneName)) return; 
               
            var saveData = Saves.GetData(ActiveProfile.id, saveId);
            if (!saveData.IsValid()) { Debug.LogError($"Invalid data at id: '{saveId}'"); return; }

            var sceneData = new SceneData(sceneName, SaveRegistry.CaptureAll());

            saveData.activeScene = sceneName;
            saveData.AddSceneData(sceneData);
            UpdateProfile(saveData, saveName, false);
        }

        /// <summary> Save current game state to the active profile's auto-save slot. </summary>
        public void AutoSave(string sceneName) {
            if (!CanSave()) return;  
            if (string.IsNullOrEmpty(sceneName)) return; 

            var sceneData = new SceneData(sceneName, SaveRegistry.CaptureAll());
            var autoSave = Saves.GetData(ActiveProfile.id, ActiveProfile.autoSave.id);
            if (autoSave.IsValid()) {
                autoSave.AddSceneData(sceneData);
                UpdateProfile(autoSave, sceneName, true);
                return;
            }

            var newData = new SaveData(Application.version, sceneName);
            newData.AddSceneData(sceneData);
            UpdateProfile(newData, sceneName, true);
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
            if (string.IsNullOrEmpty(profileId)) return;
            Saves.DeleteProfile(profileId);     
            
            if (ActiveProfile != null && profileId == ActiveProfile.id) 
                ActiveProfile = null;
            ProfilesChanged?.Invoke();
        }

        /// <summary> Return all profiles known to the repository (cached on disk). </summary>
        public List<SaveProfile> GetAllProfiles() {
            return Saves.GetAllProfiles();
        }

        public List<SaveMeta> GetAllSlotsFromActive() {
            if (!ActiveProfile.IsValid()) return new List<SaveMeta>();
            return Saves.GetAllMeta(ActiveProfile.id);
        }

        private void UpdateProfile(SaveData data, string displayName, bool isAutoSave) {
            var updatedProfile = Saves.SaveData(ActiveProfile.id, data, displayName, isAutoSave, config);

            if (updatedProfile.IsValid()) {
                ActiveProfile = updatedProfile;
                ProfilesChanged?.Invoke();
                Debug.Log("Saved Successfully !");
            }
            else { Debug.Log("Failed to Save"); }
        }

        private bool CanSave() {
            if (PendingLoadData != null) { Debug.Log("Save ignored: a load is pending"); return false; }
            if (!ActiveProfile.IsValid()) { Debug.Log("Active profile not set, can not operate on it."); return false; }
            return true;
        }
        #endregion
    }
}