using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1500)]
public class InputListener : MonoBehaviour
{
    public static InputListener Instance { get; private set; }

    [SerializeField] private List<ListenedAction> actions;

    public static event Action<GameAction> ActionPressed;
    public static event Action<GameAction> ActionReleased;

    private readonly Dictionary<GameAction, Action<InputAction.CallbackContext>> performedHandlers = new();
    private readonly Dictionary<GameAction, Action<InputAction.CallbackContext>> canceledHandlers = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        foreach (var entry in actions)
        {
            if (entry.action == null || entry.action.action == null)
                continue;

            GameAction id = entry.id;

            Action<InputAction.CallbackContext> onPerformed = _ => ActionPressed?.Invoke(id);
            Action<InputAction.CallbackContext> onCanceled = _ => ActionReleased?.Invoke(id);

            performedHandlers[id] = onPerformed;
            canceledHandlers[id] = onCanceled;

            entry.action.action.performed += onPerformed;
            entry.action.action.canceled += onCanceled;
            entry.action.action.Enable();
        }
    }

    private void OnDisable()
    {
        foreach (var entry in actions)
        {
            if (entry.action == null || entry.action.action == null)
                continue;

            if (performedHandlers.TryGetValue(entry.id, out var onPerformed))
                entry.action.action.performed -= onPerformed;

            if (canceledHandlers.TryGetValue(entry.id, out var onCanceled))
                entry.action.action.canceled -= onCanceled;

            entry.action.action.Disable();
        }

        performedHandlers.Clear();
        canceledHandlers.Clear();
    }
}

public enum GameAction
{
    Esc,
    Inventory,
    Interact
}

[Serializable]
public class ListenedAction
{
    public GameAction id;
    public InputActionReference action;
}