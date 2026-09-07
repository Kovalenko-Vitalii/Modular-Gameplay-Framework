using UnityEngine;
using Unity.Cinemachine;
using VContainer;

public sealed class CinemachineController : MonoBehaviour {
    CinemachineInputAxisController inputController;

    PauseService _pauseService;

    [Inject]
    void Construct(PauseService pauseService) => _pauseService = pauseService;

    private void Awake() => inputController = GetComponent<CinemachineInputAxisController>();

    private void OnEnable() => _pauseService.PauseChanged += OnPausedChanged;
    private void OnDisable() => _pauseService.PauseChanged -= OnPausedChanged;  
    
    private void OnPausedChanged(bool isPaused) => inputController.enabled = !isPaused;    
}