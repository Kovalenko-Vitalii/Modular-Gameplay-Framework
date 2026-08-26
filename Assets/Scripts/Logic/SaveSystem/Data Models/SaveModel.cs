using SaveSystem;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Data model for a save profile, which contains an auto-save and a list of manual saves.
/// </summary>
[Serializable]
public class SaveProfile {
    public string id;
    public string displayName;
    public long createdUtcTicks;
    public long updatedUtcTicks;
    public SaveSlotMeta autoSave = new();
    public List<SaveSlotMeta> manualSaves = new();

    public bool HasAnySave => !string.IsNullOrEmpty(autoSave?.slotId) || manualSaves.Count > 0;
    
    /// <summary>
    /// Returns id of the latest slot in profile
    /// </summary>
    public SaveSlotMeta Latest() {
        var latest = autoSave;

        foreach (var meta in manualSaves) {
            if (string.IsNullOrEmpty(latest?.slotId) || meta.updatedUtcTicks > latest.updatedUtcTicks)
                latest = meta;
        }

        return latest;
    }

    public (SaveProfile, string message, string evictedSlotId) UpdateMeta(SaveSlotData data, bool isAutoSave, SaveConfig config) {
        string evictedSlotId = null;

        try {
            var nowTicks = DateTime.UtcNow.Ticks;

            if (isAutoSave) {
                autoSave ??= new SaveSlotMeta {
                    slotId = data.slotId,
                    createdUtcTicks = nowTicks
                };
                autoSave.slotId = data.slotId;
                autoSave.displayName = displayName;
                autoSave.updatedUtcTicks = nowTicks;
            } else {
                var meta = manualSaves.FirstOrDefault(m => m.slotId == data.slotId);

                if (meta == null) {
                    if (config != null && config.maxManualSaves > 0 && manualSaves.Count >= config.maxManualSaves) {
                        if (config.limitPolicy == SlotLimitPolicy.RejectNew)
                            return (null, $"Manual save limit reached ({config.maxManualSaves}).", null);

                        var oldest = manualSaves.OrderBy(m => m.updatedUtcTicks).First();
                        evictedSlotId = oldest.slotId;
                        manualSaves.Remove(oldest);
                    }

                    meta = new SaveSlotMeta { slotId = data.slotId, createdUtcTicks = nowTicks };
                    manualSaves.Add(meta);
                }

                if (!string.IsNullOrEmpty(displayName))
                    meta.displayName = displayName;

                meta.updatedUtcTicks = nowTicks;
            }
        } catch (Exception ex) {
                return (null, "Error, could not save: " + ex.Message, null);
        }
        return (this, "Successfully saved !", evictedSlotId);
    }
}

/// <summary>
/// Meta data for a save.
/// </summary>
[Serializable]
public class SaveSlotMeta {
    public string slotId;
    public string displayName;
    public long createdUtcTicks;
    public long updatedUtcTicks;
}

/// <summary>
/// Data model for a save.
/// </summary>
[Serializable]
public class SaveSlotData {
    public string slotId;
    public string version;
    public string sceneName;
    public List<ObjectStateEntry> objectStates = new();
}

/// <summary>
/// Data model for a saveable object state entry.
/// </summary>
[Serializable]
public class ObjectStateEntry {
    public string saveId;
    public string type;
    public string json;
}