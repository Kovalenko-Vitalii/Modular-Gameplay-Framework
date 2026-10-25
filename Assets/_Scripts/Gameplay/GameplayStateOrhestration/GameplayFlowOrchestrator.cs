using Cysharp.Threading.Tasks;
using System;
using VContainer;
using VContainer.Unity;

public class GameplayFlowOrchestrator : IInitializable, IDisposable {
    GameplayStateMachine _gameplayStateMachine;

    public GameplayFlowOrchestrator(GameplayStateMachine stateMachine) => _gameplayStateMachine = stateMachine;

    public void Initialize() => _gameplayStateMachine.Start(GameplayState.Playing).Forget();

    public void Dispose() { }

}
  