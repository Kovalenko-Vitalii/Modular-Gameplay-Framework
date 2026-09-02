using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// <summary>
// Layer of abstractio over Unity Input System
// Allows other systems listen to input actions without knowing about the underlying input system
// </summary>

// !!! MAKE IT DATA DRIVEN SYSTEM LATER !!!
[DefaultExecutionOrder(-1500)]
public class InputListener : MonoBehaviour, IService {
    public static InputListener Instance { get; private set; }
    string TAG = "InputListener";

    [SerializeField] private List<ListenedAction> actions;

    public static event Action<InputAction> ActionPressed;
    public static event Action<InputAction> ActionReleased;

    private readonly Dictionary<InputAction, Action<UnityEngine.InputSystem.InputAction.CallbackContext>> performedHandlers = new();
    private readonly Dictionary<InputAction, Action<UnityEngine.InputSystem.InputAction.CallbackContext>> canceledHandlers = new();

    public void Initialize() { }

    private void Awake() {
        if (Instance != null && Instance != this) { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;

        GameLog.Log(TAG, "Initialized");
    }

    private void OnEnable() {
        foreach (var entry in actions) {
            if (entry.action == null || entry.action.action == null)
                continue;

            InputAction id = entry.id;

            Action<UnityEngine.InputSystem.InputAction.CallbackContext> onPerformed = _ => ActionPressed?.Invoke(id);
            Action<UnityEngine.InputSystem.InputAction.CallbackContext> onCanceled = _ => ActionReleased?.Invoke(id);

            performedHandlers[id] = onPerformed;
            canceledHandlers[id] = onCanceled;

            entry.action.action.performed += onPerformed;
            entry.action.action.canceled += onCanceled;
            entry.action.action.Enable();
        }
    }

    private void OnDisable() {
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