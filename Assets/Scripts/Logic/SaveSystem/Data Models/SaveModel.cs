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

        public SaveSlotMeta autoSave = new();
        public List<SaveSlotMeta> manualSaves = new();

        string IIdentifiable.Id => id;

        public bool HasAnySave => !string.IsNullOrEmpty(autoSave?.id) || manualSaves.Count > 0;

        /// <summary> Returns meta of the latest save in profile. </summary>
        public SaveSlotMeta Latest() {
            SaveSlotMeta latest = null;
            if (autoSave.IsValid()) latest = autoSave;
               
            foreach (var meta in manualSaves) {
                if (meta.IsValid() && (latest == null || meta.updatedUtcTicks > latest.updatedUtcTicks))
                    latest = meta;
            }

            return latest;
        }

        public (SaveProfile, string evictedSlotId) UpdateMeta(SaveSlotData data, string displayName, bool isAutoSave, SaveConfig config) {
            string evictedSlotId = null; // need to make it more readable !!!

            try {
                var nowTicks = DateTime.UtcNow.Ticks;

                if (isAutoSave)  {
                    autoSave ??= new SaveSlotMeta {
                        id = data.id,
                        createdUtcTicks = nowTicks
                    };
                    autoSave.id = data.id;
                    autoSave.displayName = displayName;
                    autoSave.sceneName = data.sceneName;
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

                        meta = new SaveSlotMeta { id = data.id, createdUtcTicks = nowTicks };
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
    public class SaveSlotMeta : IIdentifiable {
        public string id;
        public string displayName;
        public string sceneName;
        public long createdUtcTicks;
        public long updatedUtcTicks;

        string IIdentifiable.Id => id;
    }

    /// <summary> Data model for a save. </summary>
    [Serializable]
    public class SaveSlotData : IIdentifiable {
        public string id;
        public string version;
        public string sceneName;
        public List<ObjectStateEntry> objectStates = new();

        string IIdentifiable.Id => id;
    }

    /// <summary> Data model for a saveable object state entry. </summary>
    [Serializable]
    public class ObjectStateEntry : IIdentifiable
    {
        public string id;
        public string type;
        public string json;

        string IIdentifiable.Id => id;
    }
}