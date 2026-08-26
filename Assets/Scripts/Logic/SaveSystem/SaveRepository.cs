using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SaveSystem {
    /// <summary>
    /// Provides basic operations with save data.
    /// </summary>
    public class SaveRepository {
        private string SavesFolder => Path.Combine(Application.persistentDataPath, "Saves");
        private string ProfileFolder(string profileId) => Path.Combine(SavesFolder, profileId);
        private string ProfileIndexPath(string profileId) => Path.Combine(ProfileFolder(profileId), "index.json");
        private string ProfileSlotsFolder(string profileId) => Path.Combine(ProfileFolder(profileId), "Slots");
        private string SlotPath(string profileId, string slotId) => Path.Combine(ProfileSlotsFolder(profileId), slotId + ".json");
        
        public void EnsureFolder() => SaveFileIO.EnsureFolder(SavesFolder);

        /// <summary>
        /// Returns profile by specified profileId.
        /// </summary>
        public SaveProfile GetProfile(string profileId) => SaveFileIO.GetProfile(ProfileIndexPath(profileId)).saveProfile;

        /// <summary>
        /// Returns dataSlot by specified slotId and profileId
        /// </summary>
        public SaveSlotData GetData(string profileId, string slotId) => SaveFileIO.GetSlotData(SlotPath(profileId, slotId)).slotData;

        /// <summary>
        /// Returns latest save saveSlotId and it`s profileId.
        /// </summary>
        public (string latestProfileId, string latest, string message) GetLatestSaveInfo() {
            SaveProfile latestProfile = null;

            foreach (var profile in ListProfiles()) {
                if (!profile.HasAnySave)
                    continue;

                if (latestProfile == null || profile.updatedUtcTicks > latestProfile.updatedUtcTicks)
                    latestProfile = profile;
            }

            if (latestProfile == null) 
                return (null, null, "No latest profile found.");
            
            (string latestSlotId, string msg) = GetLatestSlotId(latestProfile.id);

            return (latestProfile.id, latestSlotId, msg);
        }

        /// <summary>
        /// Returns latest slot within specified Id.
        /// </summary>
        public (string latestSlotId, string message) GetLatestSlotId(string profileId) {
            var response = SaveFileIO.GetProfile(ProfileIndexPath(profileId));

            if (response.saveProfile == null)
                return (null, $"Profile '{profileId}' not found.");

            if (string.IsNullOrEmpty(response.saveProfile.id)) 
                return (null, $"Profile '{profileId}' not found.");        
   
            SaveSlotMeta latest = response.saveProfile.Latest();

            if (string.IsNullOrEmpty(latest?.slotId)) 
                return (null, $"Profile '{profileId}' has no saves yet.");

            return (latest.slotId, "Succesfully found latest slot");
        }

        /// <summary>
        /// Returns all valid save profiles found on disk.
        /// </summary>
        public List<SaveProfile> ListProfiles() {
            SaveFileIO.EnsureFolder(SavesFolder);
            var result = new List<SaveProfile>();

            foreach (var dir in Directory.GetDirectories(SavesFolder)) {
                var profileId = Path.GetFileName(dir);
                var indexPath = ProfileIndexPath(profileId);

                if (!File.Exists(indexPath))
                    continue;

                var response = SaveFileIO.GetProfile(indexPath);

                if (string.IsNullOrEmpty(response.saveProfile.id)) 
                    continue;
                
                result.Add(response.saveProfile);
            }

            return result;
        }

        /// <summary>
        /// Creates and activates a new save profile.
        /// </summary>
        public (SaveProfile profile, string message) CreateProfile(string displayName, SaveConfig config) {
            if (config != null && config.maxProfles > 0 && ListProfiles().Count >= config.maxProfles)
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

        /// <summary>
        /// Deletes profile with all related data.
        /// </summary>
        public bool DeleteProfile(string profileId) {
            var path = ProfileFolder(profileId);

            if (Directory.Exists(path)) {
                Directory.Delete(path, recursive: true);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Deletes specified saveSlotData from specified profile.
        /// </summary>
        public SaveProfile DeleteData(string profileId, string slotId) {
            var profilePath = ProfileIndexPath(profileId);
            var result = SaveFileIO.GetProfile(profilePath);
            var meta = result.saveProfile.manualSaves.FirstOrDefault(m => m.slotId == slotId);

            if (meta == null)
                return null;

            SaveFileIO.DeleteFile(SlotPath(profileId, slotId));
            result.saveProfile.manualSaves.Remove(meta);
            SaveFileIO.WriteProfile(profilePath, result.saveProfile);

            return result.saveProfile;
        }

        /// <summary>
        ///   
        /// </summary>
        public (SaveProfile, string) SaveData(string profileId, SaveSlotData data, string displayName, bool isAutoSave, SaveConfig saveConfig) {
            SaveProfile profile = GetProfile(profileId);

            if (profile == null)
                return (null, "Profile not found, id: " + profileId);

            if (string.IsNullOrEmpty(data.slotId))
                return (null, "Slot ID cannot be null or empty.");

            (SaveProfile updatedProfile, string message, string evictedSlotId) = profile.UpdateMeta(data, isAutoSave, saveConfig);

            if (updatedProfile != null) {
                if (evictedSlotId != null)
                    SaveFileIO.DeleteFile(SlotPath(profileId, evictedSlotId));
                
                SaveFileIO.WriteProfile(ProfileIndexPath(profileId), updatedProfile);
                SaveFileIO.WriteSlotData(SlotPath(profileId, data.slotId), data);
                return (updatedProfile, message);
            } else 
                return (null, message);   
        }
    }
}
