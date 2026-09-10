using Cysharp.Threading.Tasks;
using System.Threading;

public sealed class CutsceneState : IGameplayState {
    public GameplayState State => GameplayState.Cutscene;

    GameplayContext _context;
    public CutsceneState(GameplayContext context) => _context = context;
       
    public async UniTask Enter(CancellationToken ct) {
        _context.PauseService.RequestPause("Cutscene", true);
        _context.InputService.RequestContext("Cutscene", InputContext.Cutscene);
        await UniTask.CompletedTask;
    }

    public UniTask Exit() {
        _context.PauseService.RequestPause("Cutscene", false);
        _context.InputService.ReleaseContext("Cutscene");
        return UniTask.CompletedTask;
    }
}