using System;

public class GameModeProvider {
    string TAG = "GameModeProvider";

    public GameMode CurrentMode { get; private set; } = GameMode.Boot;
    public event Action<GameMode> ModeChanged;

    public void SetMode(GameMode newMode) {
        if (newMode == CurrentMode) return;
            
        CurrentMode = newMode;
        ModeChanged?.Invoke(newMode);
        GameLog.Log(TAG, "Mode changed to " + newMode);
    }
}

public enum GameMode {
    Boot,
    MainMenu,
    Loading,
    Gameplay
}