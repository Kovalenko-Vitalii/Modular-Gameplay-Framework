using UnityEngine;

namespace SaveSystem {
    
    /// <summary>
    /// Strategy used when a slot or profile limit is reached.
    /// RejectNew: deny creating a new save; OverwriteOlest: replace the oldest entry.
    /// </summary>
    public enum SlotLimitPolicy {
        RejectNew,
        OverwriteOlest
    }
        
    [CreateAssetMenu(menuName = "SaveSystem/SaveConfig")]
    public class SaveConfig : ScriptableObject {
        /// <summary>
        /// Maximum number of save profiles to keep. 0 = unlimited.
        /// </summary>
        [Tooltip("0 = Unlimited")] public int maxProfles = 0;

        /// <summary>
        /// Maximum number of manual saves per profile. 0 = unlimited.
        /// </summary>
        [Tooltip("0 = Unlimited")] public int maxManualSaves = 0;

        /// <summary>
        /// Policy applied when the above limits are reached.
        /// </summary>
        public SlotLimitPolicy limitPolicy = SlotLimitPolicy.RejectNew;
    }
}

