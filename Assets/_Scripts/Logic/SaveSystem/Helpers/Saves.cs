using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SaveSystem {
    /// <summary> API for save profile, meta and data management. </summary>
    public static class Saves {
        public static void EnsureFolder() => SaveFileIO.EnsureFolder(SavesFolder);

        #region Paths
        private static string SavesFolder => Path.Combine(Application.persistentDataPath, "Saves");
        private static string ProfileFolderPath(string profileId) => Path.Combine(SavesFolder, profileId);
        private static string ProfilePath(string profileId) => Path.Combine(ProfileFolderPath(profileId), "profile.json");
        private static string SavesFolderPath(string profileId) => Path.Combine(ProfileFolderPath(profileId), "Slots");
        private static string SavePath(string profileId, string slotId) => Path.Combine(SavesFolderPath(profileId), slotId + ".json");
        #endregion

        #region Profile Operations
        /// <returns> Profile with selected id, null if not found or operation failed.</returns>
        public static SaveProfile GetProfile(string profileId) {
            if (string.IsNullOrEmpty(profileId)) return null;
            return SaveFileIO.GetProfile(ProfilePath(profileId));
        }

        /// <summary> Returns list of all profiles in saves folder. </summary>
        public static List<SaveProfile> GetAllProfiles() {
            var result = new List<SaveProfile>();

            foreach (var dir in Directory.GetDirectories(SavesFolder)) {
                var profileId = Path.GetFileName(dir);
                var profilePath = ProfilePath(profileId);
                if (!File.Exists(profilePath)) continue;
                    
                var profile = SaveFileIO.GetProfile(profilePath);
                if (!profile.IsValid()) continue;
                    
                result.Add(profile);
            }

            if (result.Count == 0) Debug.Log("There are no profiles."); 
            return result;
        }

        /// <summary> Returns most recently edited profile. If nothing found returns null. </summary>
        public static SaveProfile GetLatestProfile() {
            SaveProfile latestProfile = null;

            foreach (var profile in GetAllProfiles()) {
                if (!profile.HasAnySave) continue;
                   
                if (latestProfile == null || profile.updatedUtcTicks > latestProfile.updatedUtcTicks)
                    latestProfile = profile;
            }

            if (!latestProfile.IsValid()) { Debug.Log("Could not find latest valid profile !"); return null; }
            return latestProfile;
        }

        /// <summary> Creates new profile according to policy of pasted save config. </summary>
        /// <returns> Created profile if sucseeded, null if failed. </returns>
        public static SaveProfile CreateProfile(string displayName, SaveConfig config) {
            if (config == null) return null; 
            if (string.IsNullOrEmpty(displayName)) return null; 

            if (!config.CanCreateProfile(GetAllProfiles().Count())) { Debug.Log("Could not create profile due to policy."); return null; }

            var profile = new SaveProfile(displayName, DateTime.UtcNow.Ticks); 

            SaveFileIO.EnsureFolder(ProfileFolderPath(profile.id));
            SaveFileIO.WriteProfile(ProfilePath(profile.id), profile);

            return profile;
        }

        /// <summary> v </summary>
        public static void DeleteProfile(string profileId) {
            if (string.IsNullOrEmpty(profileId)) return; 

            var path = ProfileFolderPath(profileId);
            SaveFileIO.DeleteFolder(path);
        }

        /// Write other profile functions here =>
        #endregion

        #region Data Operations
        /// <returns> SaveData of selected profile and save. </returns>
        public static SaveData GetData(string profileId, string saveId) { 
            if (string.IsNullOrEmpty(profileId)) return null; 
            if (string.IsNullOrEmpty(saveId)) return null; 

            return SaveFileIO.GetSaveData(SavePath(profileId, saveId));
        } 

        /// <summary> Deletes data in profile, and its folder. </summary>
        /// <returns> Updated profile. </returns>
        public static SaveProfile DeleteData(string profileId, string saveId) {
            if (string.IsNullOrEmpty(profileId)) return null; 
            if (string.IsNullOrEmpty(saveId)) return null; 

            var profilePath = ProfilePath(profileId);
            var profile = SaveFileIO.GetProfile(profilePath);
            if (!profile.IsValid()) return null;

            var meta = profile.manualSaves.FirstOrDefault(m => m.id == saveId);
            if (!meta.IsValid()) return null;

            SaveFileIO.DeleteFile(SavePath(profileId, saveId));
            profile.manualSaves.Remove(meta);
            SaveFileIO.WriteProfile(profilePath, profile);

            return profile;
        }

        /// <summary> </summary>
        public static SaveProfile SaveData(string profileId, SaveData data, string displayName, bool isAutoSave, SaveConfig saveConfig) {
            SaveProfile profile = GetProfile(profileId);
            if (!profile.IsValid()) return null;
               
            (SaveProfile updatedProfile, string evictedSlotId) = profile.UpdateMeta(data, displayName, isAutoSave, saveConfig);

            if (updatedProfile.IsValid()) {
                if (!string.IsNullOrEmpty(evictedSlotId))
                    SaveFileIO.DeleteFile(SavePath(profileId, evictedSlotId)); // since updated profile already dont have removed meta, just delete folder
                
                SaveFileIO.WriteProfile(ProfilePath(profileId), updatedProfile);
                SaveFileIO.WriteData(SavePath(profileId, data.id), data);
                return updatedProfile;
            }

            return null;   
        }

        /// Write other data functions here =>
        #endregion

        #region Meta Operations
        /// <returns> List of all manual meta files from selected profile. </returns>
        public static List<SaveMeta> GetAllMeta(string profileId) {
            if (string.IsNullOrEmpty(profileId)) return null;

            var profile = SaveFileIO.GetProfile(ProfilePath(profileId));
            if (!profile.IsValid()) return null;
   
            var result = new List<SaveMeta>();

            foreach (var meta in profile.manualSaves) {
                if (meta.IsValid())
                    result.Add(meta);
            }

            if (result.Count == 0) Debug.Log("There are no meta in selected file");
            return result;
        }

        /// <returns> Latest meta file from selected profile. </returns>
        public static SaveMeta GetLatestMeta(string profileId) {
            if (string.IsNullOrEmpty(profileId)) return null; 

            var profile = SaveFileIO.GetProfile(ProfilePath(profileId));
            if (!profile.IsValid()) return null;       
   
            SaveMeta latest = profile.Latest();
            if (!latest.IsValid()) return null;

            return latest;
        }

        /// Write other meta functions here =>
        #endregion
    }
}
