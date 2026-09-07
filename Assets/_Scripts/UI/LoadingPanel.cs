using UnityEngine;
using VContainer;

public class LoadingPanel : MonoBehaviour {
    [SerializeField] GameObject _panel;

    GameModeProvider _gameModeProvider;

    [Inject]
    public void Construct(GameModeProvider gameModeProvider) => _gameModeProvider = gameModeProvider;
        
    private void OnEnable() => _gameModeProvider.ModeChanged += OnModeChanged;
    private void OnDestroy() => _gameModeProvider.ModeChanged -= OnModeChanged;

    private void OnModeChanged(GameMode mode) => _panel.SetActive(mode == GameMode.Loading);       
}
