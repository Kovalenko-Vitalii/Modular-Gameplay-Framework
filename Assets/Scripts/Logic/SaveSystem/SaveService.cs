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

        [SerializeField] private SaveConfig config;

        public SaveProfile ActiveProfile { get; private set; }
        private SaveData PendingLoadData { get; set; }

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
            if (!latestMeta.IsValid() || string.IsNullOrEmpty(latestMeta.activeScene)) return null;

            PreparePendingData(profileId, latestMeta.id);
            return latestMeta.activeScene;
        }

        /// <summary> Prepares SaveService to load latest save from all profiles. </summary> 
        /// <returns> Scene name of latest save. </returns> 
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
        /// Does not apply data to the scene; caller must call ApplyPendingData afterwards.
        /// </summary>
        private void PreparePendingData(string profileId, string saveId) {
            if (PendingLoadData != null) {
                Debug.Log("LoadSlot ignored: SaveManager is busy");
                return;
            }

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
            SaveRegistry.ResetAllToDefaults(); // in case new game started. !!!

            if (!PendingLoadData.IsValid()) return;

            var data = PendingLoadData;
            PendingLoadData = null;

            var sceneData = data.GetSceneData(sceneName);
            if (sceneData == null) return;

            SaveRegistry.RestoreAll(sceneData.objectStates);

            Debug.Log($"LoadSlot('{data.id}') complete'");
            return;
        }

        #endregion

        #region Save Operations

        /// <summary> Save current game state to the active profile's auto-save slot. </summary>
        public void AutoSave(string sceneName) {
            AutoSave1(sceneName);
        }

        /// <summary> Create a new manual save slot and persist current state to it.</summary>
        public void NewManualSave(string displayName, string sceneName) {
            NewSave(displayName, sceneName);
        }

        /// <summary> Overwrite an existing manual save slot with the current state. </summary>
        public void OverwriteManual(string saveId, string sceneName, string displayName) {
            OverwriteSave(saveId, sceneName, displayName);
        }

        /// <summary>
        /// Capture current object states and persist them via the repository. On success
        /// ActiveProfile is updated; on failure a message is logged.
        /// </summary>

        // New manual save
        // 1. find latest
        // 2. create new and assign scene data from prev
        // 3. update profile
        private void NewSave(string displayName, string sceneName) {
            if (!ActiveProfile.IsValid()) {
                Debug.Log("Active profile not set, can not operate on it.");
                return;
            }

            var sceneData = new SceneData {
                sceneName = sceneName,
                objectStates = SaveRegistry.CaptureAll()
            };

            var newData = new SaveData(Application.version, sceneName);

            var latestMeta = ActiveProfile.Latest();
            if (latestMeta.IsValid()) {
                var latestData = Saves.GetData(ActiveProfile.id, latestMeta.id);

                if (latestData.IsValid()) {
                    newData.scenes = latestData.scenes;
                    newData.AddSceneData(sceneData);
                    UpdateProfile(newData, displayName, false, config);
                    return;
                }
            }

            newData.AddSceneData(sceneData);
            UpdateProfile(newData, displayName, false, config);
        }
        // Overwrite Manual
        // 1. find save
        // 2. overwrite its scene data
        // 3. update profile

        private void OverwriteSave(string saveId, string sceneName, string saveName) {
            if (!ActiveProfile.IsValid()) {
                Debug.Log("Active profile not set, can not operate on it.");
                return;
            }

            if (string.IsNullOrEmpty(saveId)) {
                Debug.LogError($"Invalid saveId: '{saveId}'");
                return;
            }

            if (string.IsNullOrEmpty(sceneName)) {
                Debug.LogError($"Invalid sceneName: '{sceneName}'");
                return;
            }

            var data = Saves.GetData(ActiveProfile.id, saveId);
            if (!data.IsValid()) {
                Debug.LogError($"Invalid data at id: '{saveId}'");
                return;
            }

            var sceneData = new SceneData {
                sceneName = sceneName,
                objectStates = SaveRegistry.CaptureAll()
            };

            data.activeScene = sceneName;
            data.AddSceneData(sceneData);
            UpdateProfile(data, saveName, false, config);
        }

        // Auto Save
        // 1. find autosave
        // 2. overwrite
        // 3. create new if dont exist
        // 4. update profile

        private void AutoSave1(string sceneName) {
            if (!ActiveProfile.IsValid()) {
                Debug.Log("Active profile not set, can not operate on it.");
                return;
            }

            if (string.IsNullOrEmpty(sceneName)) {
                Debug.LogError($"Invalid sceneName: '{sceneName}'");
                return;
            }

            var sceneData = new SceneData {
                sceneName = sceneName,
                objectStates = SaveRegistry.CaptureAll()
            };

            var autoSave = Saves.GetData(ActiveProfile.id, ActiveProfile.autoSave.id);
            if (autoSave.IsValid()) {
                autoSave.AddSceneData(sceneData);
                UpdateProfile(autoSave, sceneName, true, config);
                return;
            }

            var newData = new SaveData(Application.version, sceneName);
            newData.AddSceneData(sceneData);
            UpdateProfile(newData, sceneName, true, config);
        }

        private void SaveToActive(string saveId, string saveName, string sceneName, bool isAutoSave) {
            if (PendingLoadData != null) {
                Debug.Log("Save ignored: a load is pending");
                return;
            }

            if (!ActiveProfile.IsValid()) return;
            if (string.IsNullOrEmpty(saveName)) return;

            var latestMeta = ActiveProfile.Latest();

            var newSceneData = new SceneData {
                objectStates = SaveRegistry.CaptureAll(),
                sceneName = sceneName
            };

            if (latestMeta.IsValid()) {
                var latestData = Saves.GetData(ActiveProfile.id, latestMeta.id);

                if (latestData.IsValid()) {
                    latestData.AddSceneData(newSceneData);

                    UpdateProfile(latestData, saveName, isAutoSave, config);
                    return;
                }
            }

            var newData = new SaveData(Application.version, sceneName);

            newData.AddSceneData(newSceneData);
            UpdateProfile(newData, saveName, isAutoSave, config);
        }

        private void UpdateProfile(SaveData data, string displayName, bool isAutoSave, SaveConfig config) {
            var updatedProfile = Saves.SaveData(ActiveProfile.id, data, displayName, isAutoSave, config);

            if (updatedProfile.IsValid()) {
                ActiveProfile = updatedProfile;
                ProfilesChanged?.Invoke();
                Debug.Log("Saved Successfully !");
            }
            else {
                Debug.Log("Failed to Save");
            }
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

        public List<SaveMeta> GetAllSlotsFromActive() {
            if (!ActiveProfile.IsValid()) return new List<SaveMeta>();
            return Saves.GetAllMeta(ActiveProfile.id);
        }

        public bool CanResume() {
            return Saves.GetAllProfiles().Any(profile => profile.HasAnySave);
        }

        #endregion
    }
}