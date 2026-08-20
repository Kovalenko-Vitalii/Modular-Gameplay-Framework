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

        public static SaveProfile ReadIndex(string indexPath) {
            if (!File.Exists(indexPath))
                return new SaveProfile();

            try {
                var json = File.ReadAllText(indexPath);
                return JsonUtility.FromJson<SaveProfile>(json) ?? new SaveProfile();
            }
            catch (Exception ex) {
                Debug.LogError($"[SaveFileIO] Failed to read index at '{indexPath}': {ex.Message}");
                return new SaveProfile();
            }
        }

        public static void WriteIndex(string indexPath, SaveProfile index) =>  AtomicWrite(indexPath, JsonUtility.ToJson(index, true));

        public static SaveSlotData ReadSlot(string slotPath) {
            if (!File.Exists(slotPath))
                return null;

            try {
                var json = File.ReadAllText(slotPath);
                return JsonUtility.FromJson<SaveSlotData>(json);
            }
            catch (Exception ex) {
                Debug.LogError($"[SaveFileIO] Failed to read slot at '{slotPath}': {ex.Message}");
                return null;
            }
        }

        public static void WriteSlot(string slotPath, SaveSlotData data) => AtomicWrite(slotPath, JsonUtility.ToJson(data, true)); 

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