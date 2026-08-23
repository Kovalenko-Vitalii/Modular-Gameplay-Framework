using System;
using System.Collections.Generic;

/// <summary>
/// Data model for a save profile, which contains an auto-save and a list of manual saves.
/// </summary>
[Serializable]
public class SaveProfile {
    public string profileId;
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