using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

public class GameplayStateMachine {
    readonly Dictionary<GameplayState, IGameplayState> _states = new();

    public GameplayState CurrentState { get; private set; }

    public event Action<GameplayState> StateChanged;

    public GameplayStateMachine(IEnumerable<IGameplayState> states) {
        foreach (var state in states)
            _states.Add(state.State, state);
    }

    public async UniTask Start(GameplayState initialState,CancellationToken ct = default) {
        if (CurrentState != default)
            return;

        CurrentState = initialState;

        await _states[CurrentState].Enter(ct);

        StateChanged?.Invoke(CurrentState);
    }


    public async UniTask ChangeState(GameplayState newState, CancellationToken ct = default) { 
        if (newState == CurrentState)
            return;

        if (_states.TryGetValue(CurrentState, out var current))
            await current.Exit(); 

        CurrentState = newState;

        if (_states.TryGetValue(CurrentState, out var next))
            await next.Enter(ct);

        StateChanged?.Invoke(CurrentState);
    }
}