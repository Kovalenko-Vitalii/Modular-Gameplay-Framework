using UnityEngine;

namespace SaveSystem {
    public enum SlotLimitPolicy {
        RejectNew,
        OverwriteOlest
    }

    [CreateAssetMenu(menuName = "SaveSystem/SaveConfig")]
    public class SaveConfig : ScriptableObject {
        [Tooltip("0 = Unlimited")] public int maxProfles = 0;
        [Tooltip("0 = Unlimited")] public int maxManualSaves = 0;
        public SlotLimitPolicy limitPolicy = SlotLimitPolicy.RejectNew;
    }
}

