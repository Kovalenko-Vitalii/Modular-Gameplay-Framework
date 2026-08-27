using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SaveSystem {
    /// <summary>
    /// Provides basic operations with save data.
    /// </summary>
    public static class Saves {
        private static string SavesFolder => Path.Combine(Application.persistentDataPath, "Saves");
        private static string ProfileFolder(string profileId) => Path.Combine(SavesFolder, profileId);
        private static string ProfileIndexPath(string profileId) => Path.Combine(ProfileFolder(profileId), "index.json");
        private static string ProfileSlotsFolder(string profileId) => Path.Combine(ProfileFolder(profileId), "Slots");
        private static string SlotPath(string profileId, string slotId) => Path.Combine(ProfileSlotsFolder(profileId), slotId + ".json");
        
        public static void EnsureFolder() => SaveFileIO.EnsureFolder(SavesFolder);


        /// ==PROFILE=============================================================================================================================
        public static SaveProfile GetProfile(string profileId) => SaveFileIO.GetProfile(ProfileIndexPath(profileId));
        public static List<SaveProfile> GetAllProfiles() {
            SaveFileIO.EnsureFolder(SavesFolder);
            var result = new List<SaveProfile>();

            foreach (var dir in Directory.GetDirectories(SavesFolder)) {
                var profileId = Path.GetFileName(dir);
                var indexPath = ProfileIndexPath(profileId);

                if (!File.Exists(indexPath)) continue;
                    
                var response = SaveFileIO.GetProfile(indexPath);

                if (!H.ValidateProfile(response)) continue;
                    
                result.Add(response);
            }

            return result;
        }
        public static (string latestProfileId, string latest) GetLatestProfile() {
            SaveProfile latestProfile = null;

            foreach (var profile in GetAllProfiles()) {
                if (!profile.HasAnySave)
                    continue;

                if (latestProfile == null || profile.updatedUtcTicks > latestProfile.updatedUtcTicks)
                    latestProfile = profile;
            }

            if (latestProfile == null) 
                return (null, null);
            
            string latestSlotId = GetLatestSlotId(latestProfile.id);

            return (latestProfile.id, latestSlotId);
        }
        public static (SaveProfile profile, string message) CreateProfile(string displayName, SaveConfig config) {
            if (config != null && config.maxProfles > 0 && GetAllProfiles().Count >= config.maxProfles)
                return (null, $"Profile limit reached");

            var profileId = Guid.NewGuid().ToString("N");
            var now = DateTime.UtcNow.Ticks;

            var profile = new SaveProfile {
                id = profileId,
                displayName = displayName,
                createdUtcTicks = now,
                updatedUtcTicks = now
            };

            SaveFileIO.EnsureFolder(ProfileFolder(profileId));
            SaveFileIO.WriteProfile(ProfileIndexPath(profileId), profile);

            return (profile, "Profile successfully created");
        }
        public static bool DeleteProfile(string profileId) {
            var path = ProfileFolder(profileId);

            if (Directory.Exists(path)) {
                Directory.Delete(path, recursive: true);
                return true;
            }
            return false;
        }

        /// Write other profile functions here =>

        /// =======================================================================================================================================
        public static SaveSlotData GetData(string profileId, string saveId) => SaveFileIO.GetSlotData(SlotPath(profileId, saveId));
        public static List<SaveSlotMeta> GetAllData(string profileId) {
            var profile = SaveFileIO.GetProfile(ProfileIndexPath(profileId));

            if (!H.ValidateProfile(profile)) return null;
   
            var result = new List<SaveSlotMeta>();

            foreach (var meta in profile.manualSaves) {
                if (H.ValidateMeta(meta))
                    result.Add(meta);
            }

            return result;
        }
        public static string GetLatestSlotId(string profileId) {
            var profile = SaveFileIO.GetProfile(ProfileIndexPath(profileId));

            if (!H.ValidateProfile(profile)) return null;       
   
            SaveSlotMeta latest = profile.Latest();

            if (!H.ValidateString(latest?.id)) return null;


            return latest.id;
        }


        

        
        
        public static SaveProfile DeleteData(string profileId, string slotId) {
            var profilePath = ProfileIndexPath(profileId);
            var profile = SaveFileIO.GetProfile(profilePath);

            if (!H.ValidateProfile(profile)) return null;

            var meta = profile.manualSaves.FirstOrDefault(m => m.id == slotId);

            if (!H.ValidateMeta(meta)) return null;

            SaveFileIO.DeleteFile(SlotPath(profileId, slotId));
            profile.manualSaves.Remove(meta);
            SaveFileIO.WriteProfile(profilePath, profile);

            return profile;
        }

        public static SaveProfile SaveData(string profileId, SaveSlotData data, string displayName, bool isAutoSave, SaveConfig saveConfig) {
            SaveProfile profile = GetProfile(profileId);

            if (!H.ValidateProfile(profile)) return null;
            if (!H.ValidateData(data)) return null;
               
            (SaveProfile updatedProfile, string message, string evictedSlotId) = profile.UpdateMeta(data, displayName, isAutoSave, saveConfig);

            if (updatedProfile != null) {
                if (evictedSlotId != null)
                    SaveFileIO.DeleteFile(SlotPath(profileId, evictedSlotId));
                
                SaveFileIO.WriteProfile(ProfileIndexPath(profileId), updatedProfile);
                SaveFileIO.WriteSlotData(SlotPath(profileId, data.id), data);
                return updatedProfile;
            } else 
                return null;   
        }
    }
}
