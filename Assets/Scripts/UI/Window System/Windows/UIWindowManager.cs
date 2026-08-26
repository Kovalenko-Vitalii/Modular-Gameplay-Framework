using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-1900)]
public class UIWindowManager : MonoBehaviour {
    private const string TAG = "UIWindowManager";

    [SerializeField] private UIActionBinding[] bindings;
    [SerializeField] private UIWindowDefinition defaultWindow;

    private string pauseReason;
    private readonly List<UIWindowDefinition> stack = new();

    public IReadOnlyList<UIWindowDefinition> Stack => stack;
    public UIWindowDefinition Top => stack.Count > 0 ? stack[^1] : null;

    public event Action<UIWindowDefinition> WindowOpened;
    public event Action<UIWindowDefinition> WindowClosed;

    public void Initialize(UIScreenId ownerScreen) => pauseReason = TAG + "_" + ownerScreen;

    private void OnEnable() {
        GameStateManager.ModeChanged += HandleGameModeChanged;
        InputListener.ActionPressed += HandleActionPressed;
        HandleGameModeChanged(GameStateManager.Instance.CurrentMode);
    }

    private void OnDisable() {
        GameStateManager.ModeChanged -= HandleGameModeChanged;
        InputListener.ActionPressed -= HandleActionPressed;
    }

    private void HandleActionPressed(InputAction action) {
        if (UIScreenManager.Instance.Current?.Windows != this) return;
        HandleAction(action);
    }


    private void HandleGameModeChanged(GameMode state) {
        if (state == GameMode.Cutscene || state == GameMode.Loading)
            CloseAll();
    }

    public void OpenDefaults() {
        if (defaultWindow == null) return;
            Open(defaultWindow);
    }

    // Called by UIScreenManager, only on the currently active screen
    public void HandleAction(InputAction action) {
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
        if (Top != null)
            if (Top.closableWithEsc) 
                Close(Top);
        
        else if (fallback != null)  
            Open(fallback);
    }

    public bool IsOpen(UIWindowDefinition window) => window != null && stack.Contains(window);

    public void Open(UIWindowDefinition window) {
        if (window == null || stack.Contains(window)) 
            return;
        stack.Add(window);
        WindowOpened?.Invoke(window);
        RefreshPause();
    }

    public void Close(UIWindowDefinition window) {
        if (window == null || !stack.Remove(window)) return;
        WindowClosed?.Invoke(window);
        RefreshPause();
    }

    public void Toggle(UIWindowDefinition window) {
        if (IsOpen(window)) 
            Close(window);
        else 
            Open(window);
    }

    public void CloseAll() {
        if (stack.Count == 0)
            return;
        var closing = new List<UIWindowDefinition>(stack);
        stack.Clear();
        foreach (var w in closing) {
            WindowClosed?.Invoke(w);
        }
        RefreshPause();
    }

    private void RefreshPause() {
        if (string.IsNullOrEmpty(pauseReason)) 
            return;
        bool shouldPause = stack.Any(w => w.pausesGame);
        GameStateManager.Instance.SetPauseReason(pauseReason, shouldPause);
    }
}

[Serializable]
public class UIActionBinding {
    public InputAction action;
    public UIWindowDefinition window;
    public UIActionMode mode = UIActionMode.Toggle;
}

public enum UIActionMode {
    Toggle,
    Open,
    Close,
    Back
}