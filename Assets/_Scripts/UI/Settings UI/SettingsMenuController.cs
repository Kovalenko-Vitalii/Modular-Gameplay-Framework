using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public class SettingsMenuController : MonoBehaviour {
    [SerializeField] private UISettingsCategory settingsPanel;
    [SerializeField] private InputActionAsset inputActions;

    private VideoSettingsProvider videoSettings = new();
    private AudioSettingsProvider audioSettings;
    private GameSettingsProvider gameSettings = new();
    private ControlsSettingsProvider controlSettings;

    [Inject]
    void Construct(
        AudioSettingsProvider audioSettings) {
        this.audioSettings = audioSettings;
    }

    private void Awake() {
        InputRebindPersistence.Load(inputActions);
        controlSettings = new ControlsSettingsProvider(inputActions, () => InputRebindPersistence.Save(inputActions)); // Save the input bindings whenever a rebind occurs
    }

    private void Start() => ShowGame();

    public void ShowVideo() => settingsPanel.Build(videoSettings.BuildSettings());
    public void ShowAudio() => settingsPanel.Build(audioSettings.BuildSettings());
    public void ShowGame() => settingsPanel.Build(gameSettings.BuildSettings());
    public void ShowControls() => settingsPanel.Build(controlSettings.BuildSettings());
}