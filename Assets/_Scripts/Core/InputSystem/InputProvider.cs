using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

/// <summary>
/// Layer of abstraction over Unity Input System
/// Allows other systems to listen to input actions without knowing about the underlying input system
/// </summary>
public class InputProvider : MonoBehaviour, IInputProvider {
    [SerializeField] List<ListenedAction> actions;

    public event Action<InputActionId> Pressed;
    public event Action<InputActionId> Released;

    readonly List<(InputAction action, Action<CallbackContext> onPerformed, Action<CallbackContext> onCanceled)> subscriptions = new();

    void OnEnable() {
        foreach (var entry in actions) {
            if (entry.action?.action == null) continue;
            var id = entry.id;
            var action = entry.action.action;

            void onPerformed(CallbackContext context) => Pressed?.Invoke(id);
            void onCanceled(CallbackContext context) => Released?.Invoke(id);

            action.performed += onPerformed;
            action.canceled += onCanceled;
            subscriptions.Add((action, onPerformed, onCanceled));
        }
    }

    void OnDisable() {
        foreach (var (action, onPerformed, onCanceled) in subscriptions) {
            action.performed -= onPerformed;
            action.canceled -= onCanceled;
        }
        subscriptions.Clear();
    }
}

[Serializable]
public class ListenedAction {
    public InputActionId id;
    public InputActionReference action;
}

public enum InputActionId {
    Esc,
    Inventory,
    Interact
}