public sealed class GameplayContext {
    public PauseService PauseService { get; }
    public CursorLockController CursorLockController { get; }
    public GameplayContext(PauseService pause, CursorLockController cursorLockController) {
        PauseService = pause;
        CursorLockController = cursorLockController;
    }
}
