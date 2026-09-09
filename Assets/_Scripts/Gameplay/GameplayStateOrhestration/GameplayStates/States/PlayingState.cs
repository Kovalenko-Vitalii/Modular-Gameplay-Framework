using Cysharp.Threading.Tasks;
using System.Threading;

public sealed class PlayingState : IGameplayState {
    public GameplayState State => GameplayState.Playing;

    GameplayContext _context;
    public PlayingState(GameplayContext context) => _context = context;

    public UniTask Enter(CancellationToken ct) {
        _context.PauseService.RequestPause("GameplayState", false);
        _context.CursorLockController.LockCursor();
        return UniTask.CompletedTask;
    }

    public UniTask Exit() => UniTask.CompletedTask;
}