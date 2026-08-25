using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SaveSystem
{
    /// <summary>
    /// Provides basic operations with save data.
    /// </summary>
    public class SaveRepository {
        private string SavesFolder => Path.Combine(Application.persistentDataPath, "Saves");
        private string ProfileFolder(string profileId) => Path.Combine(SavesFolder, profileId);
        private string ProfileIndexPath(string profileId) => Path.Combine(ProfileFolder(profileId), "index.json");
        private string ProfileSlotsFolder(string profileId) => Path.Combine(ProfileFolder(profileId), "Slots");
        private string SlotPath(string profileId, string slotId) => Path.Combine(ProfileSlotsFolder(profileId), slotId + ".json");


        const string TAG = "SaveRepository";
        
        public void EnsureFolder() => SaveFileIO.EnsureFolder(SavesFolder);

        /// <summary>
        /// Returns profile by specified profileId.
        /// </summary>
        public SaveProfile GetProfile(string profileId) => SaveFileIO.GetProfile(ProfileIndexPath(profileId));

        /// <summary>
        /// Returns dataSlot by specified slotId and profileId
        /// </summary>
        public SaveSlotData GetData(string profileId, string slotId) => SaveFileIO.GetSlotData(SlotPath(profileId, slotId));

        /// <summary>
        /// Returns latest save saveSlotId and it`s profileId.
        /// </summary>
        public (string, string) GetLatestSaveInfo() {
            SaveProfile latestProfile = null;

            foreach (var profile in ListProfiles()) {
                if (!profile.HasAnySave)
                    continue;

                if (latestProfile == null || profile.updatedUtcTicks > latestProfile.updatedUtcTicks)
                    latestProfile = profile;
            }

            if (latestProfile == null) {
                GameLog.Warning(TAG, "ContinueLatestGame: no profile has any saves yet.");
                return (null, null);
            }

            string latestSlotId = GetLatestSlotId(latestProfile.id);

            return (latestProfile.id, latestSlotId);
        }

        /// <summary>
        /// Returns latest slot within specified Id.
        /// </summary>
        public string GetLatestSlotId(string profileId) {
            var profile = SaveFileIO.GetProfile(ProfileIndexPath(profileId));

            if (string.IsNullOrEmpty(profile.id)) {
                GameLog.Error(TAG, $"ContinueProfile: profile '{profileId}' not found.");
                return null;
            }

            SaveSlotMeta latest = profile.Latest();

            if (string.IsNullOrEmpty(latest?.slotId)) {
                GameLog.Warning(TAG, $"ContinueProfile: profile '{profileId}' has no saves yet.");
                return null;
            }

            return latest.slotId;
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

                var profile = SaveFileIO.GetProfile(indexPath);
                if (string.IsNullOrEmpty(profile.id)) {
                    GameLog.Warning(TAG, $"Skipping unreadable/corrupt profile folder '{profileId}'.");
                    continue;
                }

                result.Add(profile);
            }

            return result;
        }

        /// <summary>
        /// Creates and activates a new save profile.
        /// </summary>
        public SaveProfile CreateProfile(string displayName) {
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

            GameLog.Log(TAG, $"Created profile '{profileId}' ('{displayName}')");
            return profile;
        }

        /// <summary>
        /// Deletes profile with all related data.
        /// </summary>
        public void DeleteProfile(string profileId) {
            var path = ProfileFolder(profileId);

            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);

            GameLog.Log(TAG, $"Deleted profile '{profileId}'");
        }
        
        /// <summary>
        /// Deletes specified saveSlotData from specified profile.
        /// </summary>
        public SaveProfile DeleteData(string profileId, string slotId) {
            var profilePath = ProfileIndexPath(profileId);
            var profile = SaveFileIO.GetProfile(profilePath);
            var meta = profile.manualSaves.FirstOrDefault(m => m.slotId == slotId);

            if (meta == null)
                return null;

            SaveFileIO.DeleteSlot(SlotPath(profileId, slotId));
            profile.manualSaves.Remove(meta);
            SaveFileIO.WriteProfile(profilePath, profile);

            return profile;
        }

        /// <summary>
        /// 
        /// </summary>
        public (SaveProfile, string) SaveData(string profileId, SaveSlotData data, string displayName, bool isAutoSave) {
            SaveProfile profile = GetProfile(profileId);

            if (profile == null)
                return (null, "Profile not found, id: " + profileId);

            if (string.IsNullOrEmpty(data.slotId))
                return (null, "Slot ID cannot be null or empty.");

            SaveFileIO.WriteSlotData(SlotPath(profileId, data.slotId), data);

            (SaveProfile updatedProfile, string message) = profile.SaveData(data, isAutoSave);

            if (updatedProfile != null) {
                SaveFileIO.WriteProfile(ProfileIndexPath(profileId), updatedProfile);
                return (updatedProfile, message);
            } else 
                return (null, message);   
        }
    }
}
