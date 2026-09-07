using System;
using System.Collections.Generic;
using UnityEngine;

public class GameplayStateController {
    string TAG = "GameplayStateController";

    public GameplayMode CurrentMode { get; private set; } = GameplayMode.Playing;
    
    public bool IsPaused { get; private set; } = false;
    private readonly HashSet<string> pauseReasons = new();

    private GameplayMode[] pausingModes = {
        GameplayMode.Cutscene,
        GameplayMode.Dead
    };

    public event Action<bool> PauseChanged;
    public event Action<GameplayMode> ModeChanged;

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

    /// <summary> If any pause reasons are active, pause the game </summary>
    private void RecomputePause() {
        bool shouldPause = pauseReasons.Count > 0;
        if (shouldPause == IsPaused)
            return;

        IsPaused = shouldPause;
        PauseChanged?.Invoke(IsPaused);
        GameLog.Log(TAG, "Paused changed to " + IsPaused);
    }

    public void SetMode(GameplayMode newMode) {
        if (newMode == CurrentMode) return;

        CurrentMode = newMode;
        ModeChanged?.Invoke(CurrentMode);
        GameLog.Log(TAG, "Mode changed to " + CurrentMode);

        bool modeForcePause = Array.IndexOf(pausingModes, newMode) >= 0;
        SetPauseReason(TAG, modeForcePause);
    }
}

public enum GameplayMode {
    Playing,
    Cutscene,
    Dead
}