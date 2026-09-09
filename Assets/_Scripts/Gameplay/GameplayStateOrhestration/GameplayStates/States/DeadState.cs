using Cysharp.Threading.Tasks;
using System.Threading;

public sealed class DeadState : IGameplayState {
    public GameplayState State => GameplayState.Dead;

    GameplayContext _context;
    public DeadState(GameplayContext context) => _context = context;

    public async UniTask Enter(CancellationToken ct) {
        _context.PauseService.RequestPause("Death", true);
        await PlayDeathSequence(ct);
    }

    public UniTask Exit() {
        _context.PauseService.RequestPause("Death", false);
        return UniTask.CompletedTask;
    }

    private UniTask PlayDeathSequence(CancellationToken ct) {
        return UniTask.CompletedTask;
    }
}