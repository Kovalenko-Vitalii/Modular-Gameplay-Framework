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
        public static void EnsureFolder(string folderPath) => Directory.CreateDirectory(folderPath);

        public static (SaveProfile saveProfile, string message) GetProfile(string profilePath) {
            var result = ReadJson<SaveProfile>(profilePath);

            if (result.data == null)
                return result;

            if (string.IsNullOrEmpty(result.data.id))
                return (null, $"Invalid save profile at '{profilePath}': profile ID is missing.");

            return result;
        }

        public static (SaveSlotData slotData, string message) GetSlotData(string slotDataPath) {
            var result = ReadJson<SaveSlotData>(slotDataPath);

            if (result.data == null)
                return result;

            if (string.IsNullOrEmpty(result.data.slotId))
                return (null, $"Invalid save slot at '{slotDataPath}': slot ID is missing.");

            return result;
        }

        public static void WriteProfile(string profilePath, SaveProfile profile) =>  AtomicWrite(profilePath, JsonUtility.ToJson(profile, true));

        public static void WriteSlotData(string slotDataPath, SaveSlotData data) => AtomicWrite(slotDataPath, JsonUtility.ToJson(data, true)); 

        public static void DeleteFile(string slotPath) {
            if (File.Exists(slotPath))
                File.Delete(slotPath);
        }

        /// <summary>
        /// Writes to a "<path>.tmp" file first, then atomically replaces the real path.
        /// The real file is never observed in a half-written state.
        /// </summary> 
        private static void AtomicWrite(string path, string contents) {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var tmpPath = path + ".tmp";
            File.WriteAllText(tmpPath, contents);

            if (File.Exists(path))
                File.Replace(tmpPath, path, destinationBackupFileName: null);
            else
                File.Move(tmpPath, path);
        }

        private static (T data, string message) ReadJson<T>(string path) {
            if (!File.Exists(path))
                return (default, $"File does not exist at path: '{path}'");

            try {
                string json = File.ReadAllText(path);
                T data = JsonUtility.FromJson<T>(json);

                if (data == null)
                    return (default, $"Failed to deserialize data at '{path}'.");

                return (data, "Successfully read data");
            }
            catch (Exception ex) {
                return (default, $"[SaveFileIO] Failed to read '{path}': {ex.Message}");
            }
        }
    }
}