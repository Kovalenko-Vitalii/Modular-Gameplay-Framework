using SaveSystem;
using UnityEngine;
using UnityEngine.UI;

public class ResumeGameButton : MonoBehaviour {
    Button button;

    private void Awake() => button = GetComponent<Button>();
    private void Start() {
        if (SaveManager.Instance == null) {
            button.interactable = false;
            Debug.LogError("Could not link to SaveManager!");
            return;
        }

        button.interactable = SaveApi.CanContinue();
        button.onClick.AddListener(OnResumeClicked);

        SaveManager.Instance.SaveCompleted += HandleSaveChanged;
        SaveManager.Instance.LoadFailed += HandleLoadFailed;
    }

    private void OnDestroy() {
        if (button != null)
            button.onClick.RemoveListener(OnResumeClicked);

        if (SaveManager.Instance != null) {
            SaveManager.Instance.SaveCompleted -= HandleSaveChanged;
            SaveManager.Instance.LoadFailed -= HandleLoadFailed;
        }
    }

    private void OnResumeClicked() {
        button.interactable = false;
        SaveApi.ContinueLatestGame();
    }

    private void HandleSaveChanged(string slotId) => button.interactable = SaveApi.CanContinue();

    private void HandleLoadFailed(string profileId, string reason) => button.interactable = SaveApi.CanContinue();
}
