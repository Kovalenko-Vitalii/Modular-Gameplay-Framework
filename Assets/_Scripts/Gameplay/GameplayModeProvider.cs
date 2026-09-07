using System;
using VContainer;

public class GameplayModeProvider {
    string TAG = "GameplayModeProvider";

    public GameplayMode CurrentMode { get; private set; } = GameplayMode.Playing;

    private GameplayMode[] pausingModes = {
        GameplayMode.Cutscene,
        GameplayMode.Dead
    };

    public event Action<GameplayMode> ModeChanged;

    PauseService _pauseService;

    [Inject]
    void Construct(PauseService pauseService) => _pauseService = pauseService;

    public void SetMode(GameplayMode newMode) {
        if (newMode == CurrentMode) return;

        CurrentMode = newMode;
        ModeChanged?.Invoke(CurrentMode);
        GameLog.Log(TAG, "Mode changed to " + CurrentMode);

        bool modeForcePause = Array.IndexOf(pausingModes, newMode) >= 0;
        _pauseService.SetPauseReason(TAG, modeForcePause);
    }
}

public enum GameplayMode {
    Playing,
    Cutscene,
    Dead
}