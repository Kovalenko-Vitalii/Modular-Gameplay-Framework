using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        public SaveProfile(string displayName, long updatedUtcTicks) {
            id = Guid.NewGuid().ToString("N");
            this.displayName = displayName;
            createdUtcTicks = DateTime.UtcNow.Ticks;
            this.updatedUtcTicks = updatedUtcTicks;
        }


        public bool HasAnySave => !string.IsNullOrEmpty(autoSave?.id) || manualSaves.Count > 0;

        /// <summary> Returns meta of the latest save in profile. </summary>
        public SaveMeta Latest() {
            SaveMeta latest = null;
            if (autoSave.IsValid()) latest = autoSave;
               
            foreach (var meta in manualSaves) {
                if (meta.IsValid() && (latest == null || meta.updatedUtcTicks > latest.updatedUtcTicks))
                    latest = meta;
            }

            if (latest == null) Debug.Log($"There is no latest meta in profile with id: '{id}'");
            return latest;
        }

        /// <returns> Updated profile and evicted slot if exists. </returns>
        public (SaveProfile, string evictedSlotId) UpdateMeta(SaveData data, string displayName, bool isAutoSave, SaveConfig config) {
            if (config == null) return (null, null);
            if (string.IsNullOrEmpty(displayName)) return (null, null);
            if (!data.IsValid()) return (null, null);

            string evictedSlotId = null;
            var nowTicks = DateTime.UtcNow.Ticks;
            if (isAutoSave){
                autoSave ??= new SaveMeta { createdUtcTicks = nowTicks};
                autoSave.id = data.id;
                autoSave.displayName = displayName;
                autoSave.activeScene = data.activeScene;
                autoSave.updatedUtcTicks = nowTicks;
            } else {
                var meta = manualSaves.FirstOrDefault(m => m.id == data.id);

                if (!meta.IsValid()) {
                    if (!config.CanCreateProfile(manualSaves.Count)) {
                        if (config.limitPolicy == SlotLimitPolicy.RejectNew)
                            return (null, null);

                        var oldest = manualSaves.OrderBy(m => m.updatedUtcTicks).First();
                        evictedSlotId = oldest.id;
                        manualSaves.Remove(oldest);
                    }

                    meta = new SaveMeta { id = data.id, createdUtcTicks = nowTicks, activeScene = data.activeScene };
                    manualSaves.Add(meta);
                }

                meta.displayName = displayName;
                meta.updatedUtcTicks = nowTicks;
            }

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

        string IIdentifiable.Id => id;

        public SaveData(string version, string activeScene) {
            id = Guid.NewGuid().ToString("N");
            this.version = version;
            this.activeScene = activeScene;
        }

        public SceneData GetSceneData(string sceneName) => scenes.FirstOrDefault(e => e.sceneName == sceneName);

        /// <summary> Adds new scene data if it is new scene, if scene data with same scene exist - rewrites it. </summary>
        public void AddSceneData(SceneData data) {
            var existing = scenes.FirstOrDefault(e => e.sceneName == data.sceneName);
            if (existing != null)
                existing.objectStates = data.objectStates;
            else 
                scenes.Add(data);
        }   
    }

    /// <summary> Data model for scene state </summary>
    [Serializable]
    public class SceneData {
        public string sceneName;
        public List<ObjectStateEntry> objectStates = new();

        public SceneData(string sceneName, List<ObjectStateEntry> objectStates) { 
            this.sceneName = sceneName;
            this.objectStates = objectStates;
        }
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