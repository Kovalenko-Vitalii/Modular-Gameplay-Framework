using System;
using System.Collections.Generic;
using UnityEngine;

// <summary>
// Singleton class that manages the game state
// Defines current game mode and whether the game is paused
// </summary>
[DefaultExecutionOrder(-2000)] // Initializes before other systems
public class GameStateManager : MonoBehaviour {
    public static GameStateManager Instance { get; private set; }
    string TAG = "GameStateManager";

    [SerializeField] private GameMode[] pausingModes = { 
        GameMode.MainMenu, 
        GameMode.Loading,
        GameMode.Cutscene 
    };

    private readonly HashSet<string> pauseReasons = new();

    public GameMode CurrentMode { get; private set; } = GameMode.Boot;
    public bool IsPaused { get; private set; } = false;

    public static event Action<bool> PauseChanged; 
    public static event Action<GameMode> ModeChanged;


    private void Awake() {
        if (Instance != null && Instance != this) { 
            Destroy(gameObject); return; 
        }
        Instance = this;
        SetMode(GameMode.MainMenu); // Not sure if this is the best place to do this, but it works for now

        GameLog.Log(TAG, "Initialized");
    }

    // --- PUBLIC API ---

    // <summary>
    // Sets a pause reason. If active is true, the reason is added to the list of reasons to pause.
    // If active is false, the reason is removed from the list of reasons to pause.
    // </summary>
    public void SetPauseReason(string reason, bool active) { // reason could be changed to object but string works for now
        bool changed;

        if (active)
            changed = pauseReasons.Add(reason);
        else      
            changed = pauseReasons.Remove(reason);

        if (!changed)
            return;

        RecomputePause();
    }

    // <summary>
    // If any pause reasons are active, pause the game
    // </summary>
    private void RecomputePause() {
        bool shouldPause = pauseReasons.Count > 0;
        if (shouldPause == IsPaused) 
            return;
        IsPaused = shouldPause;
        PauseChanged?.Invoke(IsPaused);

        GameLog.Log(TAG, "Paused changed to " + IsPaused);
    }

    // <summary>
    // Sets game mode to selected.
    // If new mode is in the list of pausing modes game will be paused
    // </summary>
    public void SetMode(GameMode newMode) {
        if (newMode == CurrentMode) 
            return;

        CurrentMode = newMode;
        ModeChanged?.Invoke(newMode);
        GameLog.Log(TAG, "Mode changed to " + newMode);

        bool modeForcesPause = Array.IndexOf(pausingModes, newMode) >= 0; // if new mode is in list of pausing modes, then pause
        SetPauseReason(TAG, modeForcesPause);
    }
}