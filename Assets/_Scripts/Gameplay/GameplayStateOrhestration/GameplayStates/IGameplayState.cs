using Cysharp.Threading.Tasks;
using System.Threading;

public interface IGameplayState {
    GameplayState State { get; }

    UniTask Enter(CancellationToken ct);
    UniTask Exit();
}

public enum GameplayState {
    Playing,
    Cutscene,
    Dead
}