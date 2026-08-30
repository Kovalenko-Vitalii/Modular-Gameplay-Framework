using System;
using System.IO;
using UnityEngine;

namespace SaveSystem {
    /// <summary>
    /// File-layer used by the save system. Exposes read/write/delete helpers and
    /// performs atomic writes to avoid partially-written save files.
    /// </summary>
    public static class SaveFileIO {
        /// <summary> Ensure the target directory for saves exists. </summary>
        public static void EnsureFolder(string folderPath) => Directory.CreateDirectory(folderPath);

        /// <summary> Read and validate a save profile. Returns (null,message) on error. </summary>
        public static SaveProfile GetProfile(string profilePath) {
            var result = ReadJson<SaveProfile>(profilePath);
            if (!result.IsValid()) { Debug.Log($"Could not find valid profile at path: '{profilePath}'"); return null; }    
            return result;
        }

        /// <summary> Read and validate slot data. Returns (null,message) on error. </summary>
        public static SaveData GetSaveData(string savePath) {
            var data = ReadJson<SaveData>(savePath);
            if (!data.IsValid()) { Debug.Log($"Could not find valid save at path: '{savePath}'"); return null; }
            return data;
        }

        /// <summary> Serialize profile to JSON and persist atomically. </summary>
        public static void WriteProfile(string profilePath, SaveProfile profile) => AtomicWrite(profilePath, JsonUtility.ToJson(profile, true));

        /// <summary> Serialize slot data to JSON and persist atomically. </summary>
        public static void WriteData(string dataPath, SaveData data) => AtomicWrite(dataPath, JsonUtility.ToJson(data, true)); 

        /// <summary> Delete the file if it exists. No-op when missing. </summary>
        public static void DeleteFile(string path) {
            if (File.Exists(path)) { 
                File.Delete(path);
                Debug.Log($"File successfully deleted at path: '{path}'");
            }   
        }

        /// <summary> Delete the folder (and its contents) if it exists. No-op when missing. </summary>
        public static void DeleteFolder(string path) {
            if (!Directory.Exists(path)) return;

            Directory.Delete(path, recursive: true);
            Debug.Log($"Deleted folder at '{path}'.");
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

        /// <summary> Read JSON from disk and deserialize to T. Returns (default, message) on failure. </summary>
        private static T ReadJson<T>(string path) {
            if (!File.Exists(path)) return default; 

            try {
                string json = File.ReadAllText(path);

                T data = JsonUtility.FromJson<T>(json);
                if (data == null) { Debug.LogWarning($"Failed to deserialize data at '{path}'."); return default; }

                return data;
            }
            catch (Exception ex) { Debug.LogError($"Failed to read '{path}': {ex.Message}"); return default; }
        }
    }
}