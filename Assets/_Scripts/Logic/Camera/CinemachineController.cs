using UnityEngine;
using Unity.Cinemachine;
using VContainer;

public sealed class CinemachineController : MonoBehaviour {
    [SerializeField] private CinemachineInputAxisController inputController;

    GameStateManager _gameStateManager;

    [Inject]
    void Construct(GameStateManager gameStateManager) {
        _gameStateManager = gameStateManager;
    }

    private void Awake() {
        if (inputController == null)
            inputController = GetComponent<CinemachineInputAxisController>();
    }

    private void OnEnable() {
        _gameStateManager.PauseChanged += OnPausedChanged;
        inputController.enabled = !_gameStateManager.IsPaused;
    }

    private void OnDisable() => _gameStateManager.PauseChanged -= OnPausedChanged;  
    
    private void OnPausedChanged(bool isPaused) => inputController.enabled = !isPaused;    
}