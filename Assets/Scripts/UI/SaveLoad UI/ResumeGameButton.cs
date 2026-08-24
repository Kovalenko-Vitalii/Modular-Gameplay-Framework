using SaveSystem;
using UnityEngine;
using UnityEngine.UI;

public class ResumeGameButton : MonoBehaviour {
    Button button;

    private void Awake() => button = GetComponent<Button>();

    private void Start() {
        if (SaveService.Instance == null) {
            button.interactable = false;
            Debug.LogError("Could not link to SaveManager!");
            return;
        }

        button.interactable = SaveService.Instance.CanContinue();
        button.onClick.AddListener(OnResumeClicked);

        SaveService.Instance.SaveCompleted += HandleSaveChanged;
        SaveService.Instance.LoadFailed += HandleLoadFailed;
    }

    private void OnDestroy() {
        if (button != null)
            button.onClick.RemoveListener(OnResumeClicked);

        if (SaveService.Instance != null) {
            SaveService.Instance.SaveCompleted -= HandleSaveChanged;
            SaveService.Instance.LoadFailed -= HandleLoadFailed;
        }
    }

    private void OnResumeClicked() => GameFlowController.Instance.ResumeGame();

    private void HandleSaveChanged(string slotId) => button.interactable = SaveService.Instance.CanContinue();
    private void HandleLoadFailed(string profileId, string reason) => button.interactable = SaveService.Instance.CanContinue();
}
