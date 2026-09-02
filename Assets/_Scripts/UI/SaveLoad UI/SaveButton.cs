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

    private void OnEnable() => button.onClick.AddListener(HandleClick);

    private void OnDisable() => button.onClick.RemoveListener(HandleClick);

    private void HandleClick() {
        button.interactable = false;
        SaveService.Instance.AutoSave(SceneLoader.Instance.CurrentContentScene);
    }
}
