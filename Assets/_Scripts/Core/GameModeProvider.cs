using System;
using VContainer;

public class GameModeProvider {
    string TAG = "GameModeProvider";

    private GameMode[] pausingModes = { 
        GameMode.MainMenu, 
        GameMode.Loading
    };

    public GameMode CurrentMode { get; private set; } = GameMode.Boot;
    public event Action<GameMode> ModeChanged;

    PauseService _pauseService;
    [Inject]
    void Construct(PauseService pauseService) {
        _pauseService = pauseService;
    }

    public void SetMode(GameMode newMode) {
        if (newMode == CurrentMode) return;
            
        CurrentMode = newMode;
        ModeChanged?.Invoke(newMode);
        GameLog.Log(TAG, "Mode changed to " + newMode);

        bool modeForcesPause = Array.IndexOf(pausingModes, newMode) >= 0; // if new mode is in list of pausing modes, then pause
        _pauseService.SetPauseReason(TAG, modeForcesPause);
    }
}

public enum GameMode {
    Boot,
    MainMenu,
    Loading,
    Gameplay
}