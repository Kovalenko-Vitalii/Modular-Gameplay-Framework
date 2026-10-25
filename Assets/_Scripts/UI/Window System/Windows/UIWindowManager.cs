using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class UIWindowManager : MonoBehaviour {
    [SerializeField] UIActionBinding[] _uiBindings;

    public readonly List<UIWindowDefinition> stack = new();

    public event Action<UIWindowDefinition> TriggerOpen;
    public event Action<UIWindowDefinition> TriggerClose;

    public UIWindowDefinition Top => stack.Count > 0 ? stack[^1] : null;

    PauseService _pauseService;
    IInputService _inputService;
    CursorLockController _cursorLockController;

    [Inject]
    void Construct(PauseService pauseService, IInputService inputProvider, CursorLockController cursorLockController) {
        _pauseService = pauseService;
        _inputService = inputProvider;
        _cursorLockController = cursorLockController;   
    }

    private void OnEnable() => _inputService.Performed += HandleAction;
    private void OnDisable() => _inputService.Performed -= HandleAction;

    #region API

    public bool IsOpen(UIWindowDefinition window) => window != null && stack.Contains(window);

    public void Open(UIWindowDefinition window) {
        if (window == null || stack.Contains(window))
            return;
        stack.Add(window);
        TriggerOpen?.Invoke(window);
        RefreshLocks();
    }

    public void Close(UIWindowDefinition window) {
        if (window == null || !stack.Remove(window)) return;
        TriggerClose?.Invoke(window);
        RefreshLocks();
    }

    public void CloseAll() {
        if (stack.Count == 0) return;
        var closing = new List<UIWindowDefinition>(stack);
        stack.Clear();
        foreach (var w in closing) {
            TriggerClose?.Invoke(w);
        }
        RefreshLocks();
    }

    #endregion

    #region Private

    void Back(UIWindowDefinition fallback) {
        if (Top != null) {
            if (Top.ClosableWithEsc)
                Close(Top);
        } else if (fallback != null)
            Open(fallback);
    }

    void Toggle(UIWindowDefinition window) {
        if (IsOpen(window)) 
            Close(window);
        else 
            Open(window);
    }

    void HandleAction(InputActionId action) {
        foreach (var binding in _uiBindings) {
            if (binding.action != action) continue;

            switch (binding.mode) {
                case UIActionMode.Toggle: Toggle(binding.window); break;
                case UIActionMode.Open: Open(binding.window); break;
                case UIActionMode.Close: Close(binding.window); break;
                case UIActionMode.Back: Back(binding.window); break;
            }
            return;
        }
    }

    void RefreshLocks() {
        if (Top != null) {
            _pauseService.RequestPause("UI", true);
            _inputService.RequestContext("UI", InputContext.UI);
            _cursorLockController.UnlockCursor();
        } else {
            _pauseService.RequestPause("UI", false);
            _inputService.ReleaseContext("UI");
            _cursorLockController.LockCursor();
        }
    }

    #endregion
}

[Serializable]
public class UIActionBinding {
    public InputActionId action;
    public UIWindowDefinition window;
    public UIActionMode mode = UIActionMode.Toggle;
}

public enum UIActionMode {
    Toggle,
    Open,
    Close,
    Back
}