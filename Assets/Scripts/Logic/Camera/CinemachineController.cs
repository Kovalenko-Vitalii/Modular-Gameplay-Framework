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
        GameStateManager.StateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.StateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameState state)
    {
        inputController.enabled = state == GameState.Gameplay;
    }
}