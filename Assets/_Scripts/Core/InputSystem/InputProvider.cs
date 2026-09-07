using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Layer of abstraction over Unity Input System
/// Allows other systems to listen to input actions without knowing about the underlying input system
/// </summary>
public class InputProvider : MonoBehaviour, IInputProvider {
    [SerializeField] List<ListenedAction> actions;

    public event Action<InputAction> Pressed;
    public event Action<InputAction> Released;

    readonly Dictionary<InputAction, Action<UnityEngine.InputSystem.InputAction.CallbackContext>> performedHandlers = new();
    readonly Dictionary<InputAction, Action<UnityEngine.InputSystem.InputAction.CallbackContext>> canceledHandlers = new();

    void OnEnable() {
        foreach (var entry in actions) {
            if (entry.action == null || entry.action.action == null)
                continue;

            InputAction id = entry.id;

            Action<UnityEngine.InputSystem.InputAction.CallbackContext> onPerformed = _ => Pressed?.Invoke(id);
            Action<UnityEngine.InputSystem.InputAction.CallbackContext> onCanceled = _ => Released?.Invoke(id);

            performedHandlers[id] = onPerformed;
            canceledHandlers[id] = onCanceled;

            entry.action.action.performed += onPerformed;
            entry.action.action.canceled += onCanceled;
            entry.action.action.Enable();
        }
    }

    void OnDisable() {
        foreach (var entry in actions) {
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