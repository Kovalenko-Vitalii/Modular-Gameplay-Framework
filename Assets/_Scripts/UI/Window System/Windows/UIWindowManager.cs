using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class UIWindowManager : MonoBehaviour {
    [SerializeField] private UIActionBinding[] bindings;
    [SerializeField] private UIWindowDefinition defaultWindow;
    private readonly List<UIWindowDefinition> stack = new();

    public IReadOnlyList<UIWindowDefinition> Stack => stack;
    public UIWindowDefinition Top => stack.Count > 0 ? stack[^1] : null;
    public bool HasOpenWindows => stack.Count > 0;

    public event Action<UIWindowDefinition> WindowOpened;
    public event Action<UIWindowDefinition> WindowClosed;

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
      
    public void OpenDefaults() {
        if (defaultWindow != null) Open(defaultWindow);   
    }

    // Called by UIScreenManager, only on the currently active screen
    public void HandleAction(InputActionId action) {
        foreach (var binding in bindings) {
            if (binding.action != action) continue;

            switch (binding.mode) {
                case UIActionMode.Toggle: Toggle(binding.window); break;
                case UIActionMode.Open: Open(binding.window); break;
                case UIActionMode.Close: Close(binding.window); break;
                case UIActionMode.Back: HandleBack(binding.window); break;
            }
            return;
        }
    }

    private void HandleBack(UIWindowDefinition fallback) {
        if (Top != null) {
            if (Top.ClosableWithEsc)
                Close(Top);
        } else if (fallback != null)
            Open(fallback);
    }

    public bool IsOpen(UIWindowDefinition window) => window != null && stack.Contains(window);

    public void Open(UIWindowDefinition window) {
        if (window == null || stack.Contains(window)) 
            return;
        stack.Add(window);
        WindowOpened?.Invoke(window);
        RefreshLocks();
    }

    public void Close(UIWindowDefinition window) {
        if (window == null || !stack.Remove(window)) return;
        WindowClosed?.Invoke(window);
        RefreshLocks();
    }

    public void Toggle(UIWindowDefinition window) {
        if (IsOpen(window)) 
            Close(window);
        else 
            Open(window);
    }

    public void CloseAll() {
        if (stack.Count == 0) return;
        var closing = new List<UIWindowDefinition>(stack);
        stack.Clear();
        foreach (var w in closing) {
            WindowClosed?.Invoke(w);
        }
        RefreshLocks();
    }

    private void RefreshLocks() {
        if (HasOpenWindows) {
            _pauseService.RequestPause("UI", true);
            _inputService.RequestContext("UI", InputContext.UI);
            _cursorLockController.UnlockCursor();
        } else {
            _pauseService.RequestPause("UI", false);
            _inputService.ReleaseContext("UI");
            _cursorLockController.LockCursor();
        }
    }
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