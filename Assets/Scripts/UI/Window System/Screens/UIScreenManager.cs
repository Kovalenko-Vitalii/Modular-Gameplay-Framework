using System;
using UnityEngine;
using UnityEngine.Device;

public enum UIScreenId { Boot, MainMenu, Loading, Gameplay }

[DefaultExecutionOrder(-1950)] // after GameStateManager (-2000), before UIWindowManager (-1900)
public class UIScreenManager : MonoBehaviour
{
    public static UIScreenManager Instance { get; private set; }
    private const string TAG = "UIScreenManager";

    [SerializeField] private UIScreen[] screens;

    public UIScreen Current { get; private set; }
    public static event Action<UIScreenId> ScreenChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        GameStateManager.ModeChanged += HandleModeChanged;
        // Apply current mode immediately in case UI initializes after GameStateManager
        HandleModeChanged(GameStateManager.Instance.CurrentMode);
    }

    private void OnDisable()
    {
        GameStateManager.ModeChanged -= HandleModeChanged;
    }

    private void HandleModeChanged(GameMode mode)
    {
        UIScreenId? targetId = ResolveScreen(mode);
        var next = targetId.HasValue ? FindScreen(targetId.Value) : null;

        foreach (var screen in screens)
            if (screen != next) screen.Deactivate();

        if (next != null && next != Current) next.Activate();
        Current = next;

        ScreenChanged?.Invoke(targetId ?? default);
    }

    // Cutscene deliberately maps to null here: it doesn't own a screen,
    // it just means "whatever screen is active, hide its windows."
    // UIWindowManager already reacts to Cutscene/Loading independently
    // for that purpose, so ScreenManager doesn't need to do anything
    // extra for it beyond not switching screens.
    private UIScreenId? ResolveScreen(GameMode mode) => mode switch
    {
        GameMode.Boot => null,
        GameMode.MainMenu => UIScreenId.MainMenu,
        GameMode.Loading => UIScreenId.Loading,
        GameMode.Gameplay => UIScreenId.Gameplay,
        GameMode.Cutscene => Current != null ? Current.ScreenId : (UIScreenId?)null,
        _ => null
    };

    private UIScreen FindScreen(UIScreenId id)
    {
        foreach (var s in screens)
            if (s.ScreenId == id) return s;

        GameLog.Log(TAG, "No screen registered for " + id);
        return null;
    }
}