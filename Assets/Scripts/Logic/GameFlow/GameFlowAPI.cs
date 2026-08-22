public static class GameFlowApi {
    public static void StartGame(string sceneName) => GameFlowController.Instance.StartGame(sceneName);
    public static void ExitToMenu() => GameFlowController.Instance.ExitToMenu();
}