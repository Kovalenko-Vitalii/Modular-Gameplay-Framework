using System;
using System.Collections.Generic;
using UnityEngine;

// <summary>
// Singleton class that manages the game state
// </summary>
[DefaultExecutionOrder(-2000)] // Initializes before other systems
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    string TAG = "GameStateManager";

    [SerializeField] private GameMode[] pausingModes = { GameMode.MainMenu, GameMode.MainMenu, GameMode.Loading, GameMode.Cutscene }; // THIS IS POINTLESS

    private readonly HashSet<string> pauseReasons = new();

    public GameMode CurrentMode { get; private set; } = GameMode.Gameplay;
    public bool IsPaused { get; private set; } = false;

    public static event Action<bool> PauseChanged; 
    public static event Action<GameMode> ModeChanged;


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // --- API for requesting a pause ---
    public void SetPauseReason(string reason, bool active)
    {
        bool changed = active ? pauseReasons.Add(reason) : pauseReasons.Remove(reason);
        if (!changed) return;
        RecomputePause();
    }

    private void RecomputePause()
    {
        bool shouldPause = pauseReasons.Count > 0;
        if (shouldPause == IsPaused) return;
        IsPaused = shouldPause;
        GameLog.Log(TAG, "Paused changed to " + IsPaused);
        PauseChanged?.Invoke(IsPaused);
    }

    public void SetMode(GameMode newMode)
    {
        if (newMode == CurrentMode) return;

        CurrentMode = newMode;
        GameLog.Log(TAG, "Mode changed to " + newMode);
        ModeChanged?.Invoke(newMode);

        bool modeForcesPause = Array.IndexOf(pausingModes, newMode) >= 0;
        SetPauseReason(TAG, modeForcesPause);
    }
}

public enum GameMode
{
    Boot,
    MainMenu,
    Loading,
    Gameplay,
    Cutscene
}