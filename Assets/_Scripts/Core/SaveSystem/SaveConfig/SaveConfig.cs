using UnityEngine;

namespace SaveSystem {
    
    /// <summary>
    /// Policy used when a slot or profile limit is reached.
    /// RejectNew: deny creating a new save; OverwriteOlest: replace the oldest entry.
    /// </summary>
    public enum SlotLimitPolicy {
        RejectNew,
        OverwriteOldest
    }
        
    [CreateAssetMenu(menuName = "SaveSystem/SaveConfig")]
    public class SaveConfig : ScriptableObject {
        /// <summary> Maximum number of save profiles to keep. 0 = unlimited. </summary>
        [Tooltip("0 = Unlimited")] public int maxProfiles = 0;

        /// <summary> Maximum number of manual saves per profile. 0 = unlimited. </summary>
        [Tooltip("0 = Unlimited")] public int maxSaves = 0;

        /// <summary> Policy applied when the above limits are reached. </summary>
        public SlotLimitPolicy limitPolicy = SlotLimitPolicy.RejectNew;

        public bool CanCreateProfile(int currentProfiles) {
            if (maxProfiles == 0) return true; 
            return currentProfiles < maxProfiles; 
        }

        public bool CanCreateSave(int currentSaves) {
            if (maxSaves == 0) return true;
            return currentSaves < maxSaves;
        }
    }
}

