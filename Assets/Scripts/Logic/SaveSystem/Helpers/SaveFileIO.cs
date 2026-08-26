using System;
using System.IO;
using UnityEngine;

namespace SaveSystem {
    /// <summary>
    /// File-layer used by the save system. Exposes read/write/delete helpers and
    /// performs atomic writes to avoid partially-written save files.
    /// </summary>
    public static class SaveFileIO {
        /// <summary>Ensure the target directory for saves exists.</summary>
        public static void EnsureFolder(string folderPath) => Directory.CreateDirectory(folderPath);

        /// <summary>
        /// Read and validate a save profile. Returns (null,message) on error.
        /// </summary>
        public static (SaveProfile saveProfile, string message) GetProfile(string profilePath) {
            var result = ReadJson<SaveProfile>(profilePath);

            if (result.data == null)
                return result;

            if (string.IsNullOrEmpty(result.data.id))
                return (null, $"Invalid save profile at '{profilePath}': profile ID is missing.");

            return result;
        }

        /// <summary>
        /// Read and validate slot data. Returns (null,message) on error.
        /// </summary>
        public static (SaveSlotData slotData, string message) GetSlotData(string slotDataPath) {
            var result = ReadJson<SaveSlotData>(slotDataPath);

            if (result.data == null)
                return result;

            if (string.IsNullOrEmpty(result.data.slotId))
                return (null, $"Invalid save slot at '{slotDataPath}': slot ID is missing.");

            return result;
        }

        /// <summary>Serialize profile to JSON and persist atomically.</summary>
        public static void WriteProfile(string profilePath, SaveProfile profile) =>  AtomicWrite(profilePath, JsonUtility.ToJson(profile, true));

        /// <summary>Serialize slot data to JSON and persist atomically.</summary>
        public static void WriteSlotData(string slotDataPath, SaveSlotData data) => AtomicWrite(slotDataPath, JsonUtility.ToJson(data, true)); 

        /// <summary>Delete the file if it exists. No-op when missing.</summary>
        public static void DeleteFile(string slotPath) {
            if (File.Exists(slotPath))
                File.Delete(slotPath);
        }

        /// <summary>
        /// Write contents to a temporary file then replace/move it into place to
        /// guarantee the target is always either old or complete new data.
        /// </summary>
        private static void AtomicWrite(string path, string contents) {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var tmpPath = path + ".tmp";
            File.WriteAllText(tmpPath, contents);

            if (File.Exists(path))
                File.Replace(tmpPath, path, destinationBackupFileName: null); // works for desktop, fine for v1
            else
                File.Move(tmpPath, path);
        }

        /// <summary>
        /// Read JSON from disk and deserialize to T. Returns (default, message) on failure.
        /// </summary>
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