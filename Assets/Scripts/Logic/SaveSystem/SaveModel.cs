using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Data model for a save profile, which contains an auto-save and a list of manual saves.
/// </summary>
[Serializable]
public class SaveProfile {
    public SaveSlotMeta autoSave = new();
    public List<SaveSlotMeta> manualSaves = new();
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

/// <summary>
/// Provides methods to capture, restore and set default state saveable objects on the scene.
/// </summary>
public static class SaveRegistry
{
    /// <summary>
    /// Captures the state of all ISaveable objects in the scene and returns a list of WorldStateEntry objects.
    /// </summary>
    public static List<ObjectStateEntry> CaptureAll()
    {
        var saveables = FindSaveables();
        var map = new Dictionary<string, ObjectStateEntry>();

        foreach (var s in saveables) {
            var state = s.CaptureState();

            if (state == null) 
                continue;

            var entry = new ObjectStateEntry {
                saveId = s.saveId,
                type = state.GetType().AssemblyQualifiedName,
                json = JsonUtility.ToJson(state)
            };

            if (map.ContainsKey(entry.saveId))
                Debug.LogWarning($"Duplicate SaveId during capture: {entry.saveId} (overwriting previous entry)");

            map[entry.saveId] = entry;
        }

        return map.Values.ToList();
    }

    /// <summary>
    /// Restores all saveable objects to their state stored in the provided save entries.
    /// Objects not present in the save are left in their default state.
    /// </summary>
    public static void RestoreAll(List<ObjectStateEntry> savedEntries)
    {
        if (savedEntries == null) 
            return;

        var sceneSaveables = FindSaveables();

        foreach (var saveable in sceneSaveables)
            saveable.ResetToDefaultState();

        var saveableById = sceneSaveables
            .GroupBy(saveable => saveable.saveId)
            .ToDictionary(group => group.Key, group => group.First());

        foreach (var entry in savedEntries)
        {
            if (string.IsNullOrWhiteSpace(entry.saveId))
                continue;

            if (!saveableById.TryGetValue(entry.saveId, out var target))
                continue;

            var stateType = Type.GetType(entry.type);
            if (stateType == null)
                continue;

            var state = JsonUtility.FromJson(entry.json, stateType);
            target.RestoreState(state);
        }
    }

    /// <summary>
    /// Sets all ISaveable objects to default state.
    /// </summary>
    public static void ResetAllToDefaults() {
        foreach (var saveable in FindSaveables())
            saveable.ResetToDefaultState();
    }

    /// <summary>
    /// Returns all ISaveable objects on the scene.
    /// </summary>
    private static List<ISaveable> FindSaveables() {
        return UnityEngine.Object
            .FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include)
            .OfType<ISaveable>()
            .Where(s => !string.IsNullOrWhiteSpace(s.saveId))
            .ToList();
    }
}