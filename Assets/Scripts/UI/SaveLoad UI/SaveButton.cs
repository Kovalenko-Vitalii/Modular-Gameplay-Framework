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

        if (SaveManager.Instance != null) {
            SaveManager.Instance.SaveCompleted += HandleSaveCompleted;
            SaveManager.Instance.SaveFailed += HandleSaveFailed;
        }
    }

    private void OnDisable() {
        button.onClick.RemoveListener(HandleClick);

        if (SaveManager.Instance != null) {
            SaveManager.Instance.SaveCompleted -= HandleSaveCompleted;
            SaveManager.Instance.SaveFailed -= HandleSaveFailed;
        }
    }

    private void HandleClick() {
        button.interactable = false;
        SaveManager.Instance.AutoSave();
    }

    private void HandleSaveCompleted(string slotId) {
        button.interactable = true;
    }

    private void HandleSaveFailed(string slotId, string reason) {
        button.interactable = true;
    }
}
