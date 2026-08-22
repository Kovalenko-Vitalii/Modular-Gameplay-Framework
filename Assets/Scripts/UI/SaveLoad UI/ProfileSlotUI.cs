using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class ProfileSlotUI : MonoBehaviour {
    [SerializeField] Button button;
    [SerializeField] TextMeshProUGUI nameText;

    string profileId;

    public void Initialize(SaveProfile saveProfile) {
        if (saveProfile == null) {
            Debug.LogWarning("No SaveProfile assigned !"); 
            return;
        }

        profileId = saveProfile.profileId;

        if (nameText != null)
            nameText.text = saveProfile.displayName;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(LoadGame);
    }

    void LoadGame() {
        if (string.IsNullOrEmpty(profileId))
        {
            Debug.LogWarning("ProfileSlotUI: no profileId set, ignoring click.");
            return;
        }

        SaveApi.ContinueProfile(profileId);
    }
}
