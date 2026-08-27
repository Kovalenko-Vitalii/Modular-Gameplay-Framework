using System.Collections.Generic;
using UnityEngine;
using System;

namespace SaveSystem {
    /// <summary>
    /// Central registry of active ISaveable objects, plus methods to capture,
    /// restore and reset their state.
    /// </summary>
    public static class SaveRegistry
    {
        private static readonly Dictionary<string, ISaveable> saveablesById = new();

        /// <summary>
        /// Registers a saveable object. Called automatically by SaveableBehaviour.Awake()
        /// Warns and ignores the new instance if its saveId is already registered.
        /// </summary>
        public static void Register(ISaveable saveable)
        {
            if (string.IsNullOrWhiteSpace(saveable.saveId))
            {
                Debug.LogWarning($"Ignoring ISaveable with null/empty saveId on '{DescribeSource(saveable)}'.");
                return;
            }

            if (saveablesById.TryGetValue(saveable.saveId, out var existing) && !ReferenceEquals(existing, saveable))
            {
                Debug.LogWarning($"Duplicate SaveId '{saveable.saveId}' on '{DescribeSource(saveable)}'. " +
                                  "Keeping the already-registered instance; this one will not be saved/restored.");
                return;
            }

            saveablesById[saveable.saveId] = saveable;
        }

        /// <summary>
        /// Deregisters a saveable object. Called automatically by SaveableBehaviour.OnDestroy.
        /// </summary>
        public static void Deregister(ISaveable saveable)
        {
            if (string.IsNullOrWhiteSpace(saveable.saveId))
                return;

            if (saveablesById.TryGetValue(saveable.saveId, out var existing) && ReferenceEquals(existing, saveable))
                saveablesById.Remove(saveable.saveId);
        }

        /// <summary>
        /// Resets all registered saveables to default, then restores state from savedEntries.
        /// Objects not present in the save, or entries with no matching object, are left/skipped.
        /// </summary>
        public static List<ObjectStateEntry> CaptureAll()
        {
            var entries = new List<ObjectStateEntry>();

            foreach (var saveable in saveablesById.Values)
            {
                var state = saveable.CaptureState();
                if (state == null)
                    continue;

                if (!SaveTypeRegistry.TryGetKey(state.GetType(), out var typeKey))
                {
                    Debug.LogError($"State type '{state.GetType().Name}' from saveId='{saveable.saveId}' has no [SaveState] key. " +
                                    "Add [SaveState(\"...\")] to the state class. Skipping.");
                    continue;
                }

                entries.Add(new ObjectStateEntry
                {
                    saveId = saveable.saveId,
                    type = typeKey,
                    json = JsonUtility.ToJson(state)
                });
            }

            return entries;
        }

        /// <summary>
        /// Restores all saveable objects to their state stored in the provided save entries.
        /// Objects not present in the save are left in their default state.
        /// </summary>
        public static void RestoreAll(List<ObjectStateEntry> savedEntries)
        {
            if (savedEntries == null)
                return;

            ResetAllToDefaults();

            foreach (var entry in savedEntries)
            {
                if (string.IsNullOrWhiteSpace(entry.saveId))
                    continue;

                if (!saveablesById.TryGetValue(entry.saveId, out var target))
                    continue; // object don`t exist in the current scene - nothing to restore onto

                if (!SaveTypeRegistry.TryGetType(entry.type, out var stateType))
                {
                    Debug.LogWarning($"Unknown save state type '{entry.type}' for saveId='{entry.saveId}'. " +
                                      "The type may have been renamed or removed. Skipping this entry.");
                    continue;
                }

                object state;
                try
                {
                    state = JsonUtility.FromJson(entry.json, stateType);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to deserialize save state for saveId='{entry.saveId}', type='{entry.type}': {ex.Message}");
                    continue;
                }

                if (state == null)
                {
                    Debug.LogWarning($"Deserialized null state for saveId='{entry.saveId}', type='{entry.type}'. Skipping.");
                    continue;
                }

                target.RestoreState(state);
            }
        }

        //// <summary>
        /// Resets all registered saveables to their default state.
        /// </summary>
        public static void ResetAllToDefaults()
        {
            foreach (var saveable in saveablesById.Values)
                saveable.ResetToDefaultState();
        }

        private static string DescribeSource(ISaveable saveable)
        {
            if (saveable is UnityEngine.Object unityObj)
                return unityObj.name;

            return saveable.GetType().Name;
        }
    }
}