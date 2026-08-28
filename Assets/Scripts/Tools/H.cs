using SaveSystem;
using UnityEngine;

public static class H {
       public static bool ValidateProfile(SaveProfile profile) {
            if (profile == null || string.IsNullOrEmpty(profile.id)) {
                Debug.LogError($"Invalid data (index: '{profile?.id}'");
                return false;
            }
            return true;
        }

        public static bool ValidateData(SaveSlotData data) {
            if (data == null || string.IsNullOrEmpty(data.id)) {
                Debug.LogError($"Invalid data (index: '{data?.id}'");
                return false;
            }
                
            return true;
        }

        public static bool ValidateMeta(SaveSlotMeta meta) {
            if (meta == null || string.IsNullOrEmpty(meta.id)) {
                Debug.LogError($"Invalid data (index: '{meta?.id}'");
                return false;
            }
                
            return true;
        }

        public static bool ValidateString(string id) {
            if (string.IsNullOrEmpty(id)) {
                Debug.LogError($"Invalid id: '{id}'");
                return false;
            }
                
            return true;
        }
}
