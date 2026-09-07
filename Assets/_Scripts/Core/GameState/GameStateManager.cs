using System;
using System.Collections.Generic;

/// <summary>
/// Global game state provider.
/// </summary>
public class GameStateManager {
    string TAG = "GameStateManager";

    private GameMode[] pausingModes = { 
        GameMode.MainMenu, 
        GameMode.Loading
    };

    private readonly HashSet<string> pauseReasons = new();

    public GameMode CurrentMode { get; private set; } = GameMode.Boot;
    public bool IsPaused { get; private set; } = false;

    public event Action<bool> PauseChanged; 
    public event Action<GameMode> ModeChanged;

    /// --- PUBLIC API ---

    /// <summary>
    /// Sets a pause reason. If active is true, the reason is added to the list of reasons to pause.
    /// If active is false, the reason is removed from the list of reasons to pause.
    /// </summary>
    public void SetPauseReason(string reason, bool active) { // !!! reason could be changed to object but string works for now !!!
        bool changed;

        if (active)
            changed = pauseReasons.Add(reason);
        else      
            changed = pauseReasons.Remove(reason);

        if (!changed)
            return;

        RecomputePause();
    }

    /// <summary>
    /// If any pause reasons are active, pause the game
    /// </summary>
    private void RecomputePause() {
        bool shouldPause = pauseReasons.Count > 0;
        if (shouldPause == IsPaused) 
            return;

        IsPaused = shouldPause;
        PauseChanged?.Invoke(IsPaused);
        GameLog.Log(TAG, "Paused changed to " + IsPaused);
    }

    public void SetMode(GameMode newMode) {
        if (newMode == CurrentMode) return;
            
        CurrentMode = newMode;
        ModeChanged?.Invoke(newMode);
        GameLog.Log(TAG, "Mode changed to " + newMode);

        bool modeForcesPause = Array.IndexOf(pausingModes, newMode) >= 0; // if new mode is in list of pausing modes, then pause
        SetPauseReason(TAG, modeForcesPause);
    }
}

public enum GameMode {
    Boot,
    MainMenu,
    Loading,
    Gameplay
}