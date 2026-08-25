using SaveSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public SaveSlotMeta Latest() {
        var latest = autoSave;
        foreach (var meta in manualSaves) {
            if (string.IsNullOrEmpty(latest?.slotId) || meta.updatedUtcTicks > latest.updatedUtcTicks)
                latest = meta;
        }

        if (latest != null)
            return latest;

        return null;
    }
    public (SaveProfile, string) SaveData(SaveSlotData data, bool isAutoSave) {
        try {   
            var nowTicks = DateTime.UtcNow.Ticks;
            if (isAutoSave) {
                autoSave ??= new SaveSlotMeta
                {
                    slotId = data.slotId,
                    createdUtcTicks = nowTicks
                };
                autoSave.slotId = data.slotId;
                autoSave.displayName = displayName;
                autoSave.updatedUtcTicks = nowTicks;
            }
            else
            {
                var meta = manualSaves.FirstOrDefault(m => m.slotId == data.slotId);
                if (meta == null)
                {
                    meta = new SaveSlotMeta { slotId = data.slotId, createdUtcTicks = nowTicks };
                    manualSaves.Add(meta);
                }

                if (!string.IsNullOrEmpty(displayName))
                    meta.displayName = displayName;

                meta.updatedUtcTicks = nowTicks;
            }
        } catch (Exception ex) {
                return (null, "Error, could not save: " + ex.Message);
        }
        return (this, "Successfully saved !");
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