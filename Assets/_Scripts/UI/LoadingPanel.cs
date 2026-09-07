using UnityEngine;
using VContainer;

public class LoadingPanel : MonoBehaviour {
    [SerializeField] GameObject _panel;

    GameStateManager _gameStateManager;

    [Inject]
    public void Construct(GameStateManager gameStateManager) => _gameStateManager = gameStateManager;
        
    private void OnEnable() => _gameStateManager.ModeChanged += OnModeChanged;
    private void OnDestroy() => _gameStateManager.ModeChanged -= OnModeChanged;

    private void OnModeChanged(GameMode mode) => _panel.SetActive(mode == GameMode.Loading);       
}
