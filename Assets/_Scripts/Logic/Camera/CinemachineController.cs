using UnityEngine;
using Unity.Cinemachine;

public sealed class CinemachineController : MonoBehaviour
{
    [SerializeField] private CinemachineInputAxisController inputController;

    private void Awake()
    {
        if (inputController == null)
            inputController = GetComponent<CinemachineInputAxisController>();
    }

    private void OnEnable()
    {
        GameStateManager.PauseChanged += OnPausedChanged;
        if (GameStateManager.Instance != null)
            inputController.enabled = !GameStateManager.Instance.IsPaused;
    }

    private void OnDisable() => GameStateManager.PauseChanged -= OnPausedChanged;   
    private void OnPausedChanged(bool isPaused) => inputController.enabled = !isPaused;    
}