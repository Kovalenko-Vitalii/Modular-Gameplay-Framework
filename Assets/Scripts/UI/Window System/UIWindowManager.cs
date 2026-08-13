using System;
using UnityEngine;

// <summary>
// Singleton class that manages the currently open UI window
// </summary>
[DefaultExecutionOrder(-1900)]
public class UIWindowManager : MonoBehaviour
{
    public static UIWindowManager Instance { get; private set; }

    private const string TAG = "UIWindowManager";

    public UIWindowId Current { get; private set; } = UIWindowId.None;

    public static event Action<UIWindowId> WindowChanged;

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
        InputListener.ActionPressed += OnActionPressed;
        // optional: react if some other system forces a state that should close windows
        GameStateManager.ModeChanged += OnGameStateChanged;
    }

    private void OnDisable()
    {
        InputListener.ActionPressed -= OnActionPressed;
        GameStateManager.ModeChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameMode state)
    {
        if (state == GameMode.Cutscene || state == GameMode.Loading)
            CloseWindow(); // don't leave a window open under a cutscene
    }

    private void OnActionPressed(GameAction action)
    {
        switch (action)
        {
            case GameAction.Esc:
                HandleEsc();
                break;
        }
    }

    private void HandleEsc()
    {
        if (Current == UIWindowId.None)
        {
            OpenWindow(UIWindowId.Esc);
            GameStateManager.Instance.SetPaused(true);
        }
        else
        {
            CloseWindow();
            GameStateManager.Instance.SetPaused(false);
        }
    }

    public void ToggleWindow(UIWindowId window)
    {
        if (Current == window)
            CloseWindow();
        else
            OpenWindow(window);
    }

    public void OpenWindow(UIWindowId window) => SetWindow(window);

    public void CloseWindow() => SetWindow(UIWindowId.None);

    private void SetWindow(UIWindowId window)
    {
        if (window == Current)
            return;

        Current = window;
        GameLog.Log(TAG, "Window changed to " + window);
        WindowChanged?.Invoke(window);
    }
}

public enum UIWindowId
{
    None,
    Esc,
    Inventory,
    MainMenu
}