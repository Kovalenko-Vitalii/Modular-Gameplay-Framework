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
        /// <summary> v </summary>
        public static SaveProfile GetProfile(string profileId) => SaveFileIO.GetProfile(ProfilePath(profileId));

        /// <summary> v </summary>
        public static List<SaveProfile> GetAllProfiles() {
            EnsureFolder();
            var result = new List<SaveProfile>();

            foreach (var dir in Directory.GetDirectories(SavesFolder)) {
                var profileId = Path.GetFileName(dir);
                var profilePath = ProfilePath(profileId);

                if (!File.Exists(profilePath)) continue;
                    
                var profile = SaveFileIO.GetProfile(profilePath);

                if (!profile.IsValid()) continue;
                    
                result.Add(profile);
            }

            return result;
        }

        /// <summary> v </summary>
        public static SaveProfile GetLatestProfile() {
            SaveProfile latestProfile = null;

            foreach (var profile in GetAllProfiles()) {
                if (!profile.HasAnySave) continue;
                   
                if (latestProfile == null || profile.updatedUtcTicks > latestProfile.updatedUtcTicks)
                    latestProfile = profile;
            }

            return latestProfile;
        }

        /// <summary> vv </summary>
        public static SaveProfile CreateProfile(string displayName, SaveConfig config) {
            if (config != null && config.maxProfles > 0 && GetAllProfiles().Count >= config.maxProfles)
                return null; // maybe move somewhere

            var profile = new SaveProfile {
                id = Guid.NewGuid().ToString("N"),
                displayName = displayName,
                createdUtcTicks = DateTime.UtcNow.Ticks,
                updatedUtcTicks = DateTime.UtcNow.Ticks
            };

            SaveFileIO.EnsureFolder(ProfileFolderPath(profile.id));
            SaveFileIO.WriteProfile(ProfilePath(profile.id), profile);

            return profile;
        }

        /// <summary> v </summary>
        public static void DeleteProfile(string profileId) {
            if (string.IsNullOrEmpty(profileId)) return;

            var path = ProfileFolderPath(profileId);
            SaveFileIO.DeleteFile(path);
        }

        /// Write other profile functions here =>
        #endregion

        #region Data Operations
        /// <summary> v </summary>
        public static SaveData GetData(string profileId, string saveId) => SaveFileIO.GetSlotData(SavePath(profileId, saveId));

        /// <summary> v </summary>
        public static SaveProfile DeleteData(string profileId, string saveId) {
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
                SaveFileIO.WriteSlotData(SavePath(profileId, data.id), data);
                return updatedProfile;
            } else 
                return null;   
        }

        /// Write other data functions here =>
        #endregion

        #region Meta Operations
        /// <summary> v </summary>
        public static List<SaveMeta> GetAllMeta(string profileId) {
            var profile = SaveFileIO.GetProfile(ProfilePath(profileId));
            if (!profile.IsValid()) return null;
   
            var result = new List<SaveMeta>();

            foreach (var meta in profile.manualSaves) {
                if (meta.IsValid())
                    result.Add(meta);
            }

            return result;
        }

        /// <summary> </summary>
        public static SaveMeta GetLatestMeta(string profileId) {
            var profile = SaveFileIO.GetProfile(ProfilePath(profileId));
            if (!profile.IsValid()) return null;       
   
            SaveMeta latest = profile.Latest();
            if (!profile.IsValid()) return null;

            return latest;
        }

        /// Write other meta functions here =>
        #endregion
    }
}
