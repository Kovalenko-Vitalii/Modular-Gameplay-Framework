public sealed class GameplayContext {
    public PauseService PauseService { get; }
    public CursorLockController CursorLockController { get; }
    public IInputService InputService { get; }
    public GameplayContext(PauseService pause, CursorLockController cursorLockController, IInputService inputService) {
        PauseService = pause;
        CursorLockController = cursorLockController;
        InputService = inputService;
    }
}
