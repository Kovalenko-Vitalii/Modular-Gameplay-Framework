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

    [SerializeField] private GameMode[] pausingModes = { GameMode.MainMenu, GameMode.Loading, GameMode.Cutscene };

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
    public void SetPaused(bool paused)
    {
        if (paused == IsPaused) return;
        IsPaused = paused;
        GameLog.Log(TAG, "Paused changed to " + IsPaused);
        PauseChanged?.Invoke(IsPaused);
    }

    public void SetMode(GameMode newMode)
    {
        if (newMode == CurrentMode) return;

        CurrentMode = newMode;
        GameLog.Log(TAG, "Mode changed to " + newMode);
        ModeChanged?.Invoke(newMode);

        bool shouldPause = Array.IndexOf(pausingModes, newMode) >= 0;
        SetPaused(shouldPause);
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