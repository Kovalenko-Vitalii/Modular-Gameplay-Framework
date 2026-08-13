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
    }

    private void OnDisable()
    {
        InputListener.ActionPressed -= OnActionPressed;
    }

    private void OnActionPressed(GameAction action)
    {
        switch (action)
        {
            case GameAction.Esc:
                HandleEsc();
                break;

            case GameAction.Inventory:
                ToggleWindow(UIWindowId.Inventory);
                break;
        }
    }

    private void HandleEsc()
    {
        if (Current == UIWindowId.None)
            OpenWindow(UIWindowId.Esc);
        else
            CloseWindow();
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