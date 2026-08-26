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

        button.interactable = SaveService.Instance.CanResume();
        button.onClick.AddListener(OnResumeClicked);

        SaveService.Instance.ProfilesChanged += HandleSaveChanged;
    }

    private void OnDestroy() {
        if (button != null)
            button.onClick.RemoveListener(OnResumeClicked);

        if (SaveService.Instance != null) {
            SaveService.Instance.ProfilesChanged -= HandleSaveChanged;
        }
    }

    private void OnResumeClicked() => GameFlowController.Instance.ResumeGame();

    private void HandleSaveChanged() => button.interactable = SaveService.Instance.CanResume();
}
