using SaveSystem;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Basic UI script to test save functionality.
/// !!!
/// NOT FINAL VERSION
/// !!!
/// </summary>
public class SaveButton : MonoBehaviour {
    [SerializeField] private Button button;

    private void Reset() => button = GetComponent<Button>();

    private void Awake() {
        if (button == null)
            button = GetComponent<Button>();
    }

    private void OnEnable() {
        button.onClick.AddListener(HandleClick);

        if (SaveService.Instance != null) {
            SaveService.Instance.SaveCompleted += HandleSaveCompleted;
            SaveService.Instance.SaveFailed += HandleSaveFailed;
        }
    }

    private void OnDisable() {
        button.onClick.RemoveListener(HandleClick);

        if (SaveService.Instance != null) {
            SaveService.Instance.SaveCompleted -= HandleSaveCompleted;
            SaveService.Instance.SaveFailed -= HandleSaveFailed;
        }
    }

    private void HandleClick() {
        button.interactable = false;
        SaveService.Instance.AutoSave();
    }

    private void HandleSaveCompleted(string slotId) {
        button.interactable = true;
    }

    private void HandleSaveFailed(string slotId, string reason) {
        button.interactable = true;
    }
}
