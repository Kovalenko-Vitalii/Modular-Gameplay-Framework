using SaveSystem;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// Basic UI script to test save functionality.
/// !!!
/// NOT FINAL VERSION
/// !!!
/// </summary>
public class SaveButton : MonoBehaviour {
    [SerializeField] private Button button;

    SceneLoader _sceneLoader;
    SaveService _saveService;

    [Inject]
    void Construct(SceneLoader sceneLoader, SaveService saveService) {
        _sceneLoader = sceneLoader;
        _saveService = saveService;
    }

    private void Reset() => button = GetComponent<Button>();

    private void Awake() {
        if (button == null)
            button = GetComponent<Button>();
    }

    private void OnEnable() => button.onClick.AddListener(HandleClick);

    private void OnDisable() => button.onClick.RemoveListener(HandleClick);

    private void HandleClick() {
        button.interactable = false;
        _saveService.AutoSave(_sceneLoader.CurrentContentScene);
    }
}
