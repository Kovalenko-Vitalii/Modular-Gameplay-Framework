using System;
using System.Collections.Generic;
using System.Linq;

namespace SaveSystem {
    /// <summary> Data model for a save profile, contains an auto-save and a list of manual saves. </summary>
    [Serializable]
    public class SaveProfile : IIdentifiable{
        public string id;
        public string displayName;
        public long createdUtcTicks;
        public long updatedUtcTicks;

        public SaveMeta autoSave = new();
        public List<SaveMeta> manualSaves = new();

        string IIdentifiable.Id => id;

        public bool HasAnySave => !string.IsNullOrEmpty(autoSave?.id) || manualSaves.Count > 0;

        /// <summary> Returns meta of the latest save in profile. </summary>
        public SaveMeta Latest() {
            SaveMeta latest = null;
            if (autoSave.IsValid()) latest = autoSave;
               
            foreach (var meta in manualSaves) {
                if (meta.IsValid() && (latest == null || meta.updatedUtcTicks > latest.updatedUtcTicks))
                    latest = meta;
            }

            return latest;
        }

        public (SaveProfile, string evictedSlotId) UpdateMeta(SaveData data, string displayName, bool isAutoSave, SaveConfig config) {
            string evictedSlotId = null; // need to make it more readable !!!

            try {
                var nowTicks = DateTime.UtcNow.Ticks;

                if (isAutoSave)  {
                    autoSave ??= new SaveMeta {
                        id = data.id,
                        createdUtcTicks = nowTicks
                    };
                    autoSave.id = data.id;
                    autoSave.displayName = displayName;
                    autoSave.activeScene = data.activeScene;
                    autoSave.updatedUtcTicks = nowTicks;
                } else  {
                    var meta = manualSaves.FirstOrDefault(m => m.id == data.id);

                    if (meta == null) {
                        if (config != null && config.maxManualSaves > 0 && manualSaves.Count >= config.maxManualSaves) {
                            if (config.limitPolicy == SlotLimitPolicy.RejectNew)
                                return (null, null);

                            var oldest = manualSaves.OrderBy(m => m.updatedUtcTicks).First();
                            evictedSlotId = oldest.id;
                            manualSaves.Remove(oldest);
                        }

                        meta = new SaveMeta { id = data.id, createdUtcTicks = nowTicks };
                        manualSaves.Add(meta);
                    }

                    if (!string.IsNullOrEmpty(displayName))
                        meta.displayName = displayName;

                    meta.updatedUtcTicks = nowTicks;
                }
            }
            catch { throw; }
               
            return (this, evictedSlotId);
        }
    }

    /// <summary> Meta data for a save. </summary>
    [Serializable]
    public class SaveMeta : IIdentifiable {
        public string id;
        public string displayName;
        public string activeScene;
        public long createdUtcTicks;
        public long updatedUtcTicks;

        string IIdentifiable.Id => id;
    }

    /// <summary> Data model for a save. </summary>
    [Serializable]
    public class SaveData : IIdentifiable {
        public string id;
        public string version;
        public string activeScene;
        public List<SceneData> scenes = new();

        public SaveData(string version, string activeScene, List<SceneData> scenes) {
            id = Guid.NewGuid().ToString("N");
            this.version = version;
            this.activeScene = activeScene;
            this.scenes = scenes;
        }

        public SaveData(string version, string activeScene) {
            id = Guid.NewGuid().ToString("N");
            this.version = version;
            this.activeScene = activeScene;
        }

        /// <summary> Adds new scene data if it is new scene, if scene data with same scene exist - rewrites it. </summary>
        public void AddSceneData(SceneData data) {
            var existing = scenes.FirstOrDefault(e => e.sceneName == data.sceneName);
            if (existing != null)
                existing.objectStates = data.objectStates;
            else 
                scenes.Add(data);
        }

        public SceneData GetSceneData(string sceneName) {
            return scenes.FirstOrDefault(e => e.sceneName == sceneName);
        }

        string IIdentifiable.Id => id;
    }

    /// <summary> Data model for scene state </summary>
    [Serializable]
    public class SceneData {
        public string sceneName;
        public List<ObjectStateEntry> objectStates = new();
    }

    /// <summary> Data model for a saveable object state entry. </summary>
    [Serializable]
    public class ObjectStateEntry : IIdentifiable {
        public string id;
        public string type;
        public string json;

        string IIdentifiable.Id => id;
    }
}