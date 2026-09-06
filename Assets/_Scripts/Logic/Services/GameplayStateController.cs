using System;

public enum GameplayState { 
    Playing,
    Cutscene, 
    Dead 
}

public class GameplayStateController {
    public GameplayState Current { get; private set; } = GameplayState.Playing;
    public event Action<GameplayState, GameplayState> StateChanged;

    public void SetState(GameplayState next) {
        if (next == Current) return;
        var prev = Current;
        Current = next;
        StateChanged?.Invoke(prev, next);
    }
}