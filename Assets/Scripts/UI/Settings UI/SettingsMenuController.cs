using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsMenuController : MonoBehaviour
{
    [SerializeField] private UISettingsCategory settingsPanel;
    [SerializeField] private InputActionAsset inputActions;

    private VideoSettingsProvider video = new();
    private AudioSettingsProvider audio = new();
    private GameSettingsProvider game = new();
    private ControlsSettingsProvider controls;

    private void Awake()
    {
        InputRebindPersistence.Load(inputActions);
        controls = new ControlsSettingsProvider(inputActions, () => InputRebindPersistence.Save(inputActions)); // Save the input bindings whenever a rebind occurs
    }

    private void Start() => ShowGame();

    public void ShowVideo() => settingsPanel.Build(video.BuildSettings());
    public void ShowAudio() => settingsPanel.Build(audio.BuildSettings());
    public void ShowGame() => settingsPanel.Build(game.BuildSettings());
    public void ShowControls() => settingsPanel.Build(controls.BuildSettings());
}