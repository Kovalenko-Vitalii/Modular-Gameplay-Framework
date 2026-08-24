using System;
using System.IO;
using UnityEngine;

namespace SaveSystem {
    /// <summary>
    /// Disk I/O for the save system. 
    /// Every write is atomic (temp file + replace) so a crash
    /// or power loss mid-write can't leave a corrupt slot or index on disk.
    /// </summary>
    public static class SaveFileIO {
        public static void EnsureFolder(string folderPath) {
            Directory.CreateDirectory(folderPath);
        }

        /// <summary>
        /// Returns SaveProfile located at specified path.
        /// If nothing found creates blank one and returns it.
        /// </summary>
        public static SaveProfile GetProfile(string profilePath) {
            if (!File.Exists(profilePath))
                return new SaveProfile();

            try {
                var json = File.ReadAllText(profilePath);
                return JsonUtility.FromJson<SaveProfile>(json) ?? new SaveProfile();
            }
            catch (Exception ex) {
                Debug.LogError($"[SaveFileIO] Failed to read index at '{profilePath}': {ex.Message}");
                return new SaveProfile();
            }
        }

        public static void WriteProfile(string profilePath, SaveProfile profile) =>  AtomicWrite(profilePath, JsonUtility.ToJson(profile, true));

        /// <summary>
        /// Returns SaveSlotData located at specified path.
        /// If nothing found creates blank one and returns it.
        /// </summary>
        public static SaveSlotData GetSlotData(string slotDataPath) {
            if (!File.Exists(slotDataPath))
                return null;

            try {
                var json = File.ReadAllText(slotDataPath);
                return JsonUtility.FromJson<SaveSlotData>(json);
            }
            catch (Exception ex) {
                Debug.LogError($"[SaveFileIO] Failed to read slot at '{slotDataPath}': {ex.Message}");
                return null;
            }
        }

        public static void WriteSlotData(string slotDataPath, SaveSlotData data) => AtomicWrite(slotDataPath, JsonUtility.ToJson(data, true)); 

        public static void DeleteSlot(string slotPath) {
            if (File.Exists(slotPath))
                File.Delete(slotPath);
        }

        /// <summary>
        /// Writes to a "<path>.tmp" file first, then atomically replaces the real path.
        /// The real file is never observed in a half-written state.
        /// </summary> 
        private static void AtomicWrite(string path, string contents) {
            var tmpPath = path + ".tmp";
            File.WriteAllText(tmpPath, contents);

            if (File.Exists(path))
                File.Replace(tmpPath, path, destinationBackupFileName: null);
            else
                File.Move(tmpPath, path);
        }
    }
}