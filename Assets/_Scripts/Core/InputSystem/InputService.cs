using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Layer of abstraction over Unity Input System
/// Allows other systems to listen to input actions without knowing about the underlying input system
/// </summary>
public class InputService : IInputService {
    static readonly InputContext[] Priority = { InputContext.Cutscene, InputContext.UI, InputContext.Gameplay };

    readonly InputActionMap _global, _player, _ui, _windows;
    readonly Dictionary<string, InputContext> _requests = new();
    InputContext _current;

    public event Action<InputActionId> Performed;
    public event Action<InputActionId> Canceled;

    public Vector2 Move => _player.FindAction("Move")?.ReadValue<Vector2>() ?? Vector2.zero;
    public Vector2 Look => _player.FindAction("Look")?.ReadValue<Vector2>() ?? Vector2.zero;

    public InputService(InputActionAsset asset) {
        _global = asset.FindActionMap("Global");
        _player = asset.FindActionMap("Player");
        _ui = asset.FindActionMap("UI");
        _windows = asset.FindActionMap("Windows");

        // The asset IS the source of truth. No manual list to keep in sync —
        // just name every InputActionId enum value after the real action name.
        foreach (var map in asset.actionMaps)
            foreach (var action in map.actions) {
                if (!Enum.TryParse<InputActionId>(action.name, true, out var id)) {
                    GameLog.Warning("InputService", $"Action '{action.name}' has no matching InputActionId.");
                    continue;
                }
                action.performed += _ => Performed?.Invoke(id);
                action.canceled += _ => Canceled?.Invoke(id);
            }

        _global.Enable();
        Apply(InputContext.Gameplay);
    }

    public void RequestContext(string key, InputContext context) {
        _requests[key] = context;
        Recompute();
    }

    public void ReleaseContext(string key) {
        _requests.Remove(key);
        Recompute();
    }

    void Recompute() {
        foreach (var ctx in Priority)
            if (_requests.ContainsValue(ctx)) { Apply(ctx); return; }
        Apply(InputContext.Gameplay);
    }

    void Apply(InputContext context) {
        if (_current == context) return;
        _current = context;

        _player.Disable();
        _ui.Disable();
        _windows.Disable();

        switch (context) {
            case InputContext.Gameplay: _player.Enable(); _windows.Enable(); break;
            case InputContext.UI: _ui.Enable(); _windows.Enable(); break;
            case InputContext.Cutscene: break;
        }
    }
}

public enum InputContext {
    Gameplay,
    UI,
    Cutscene
}

public enum InputActionId {
    Esc,
    Inventory,
    Interact
}