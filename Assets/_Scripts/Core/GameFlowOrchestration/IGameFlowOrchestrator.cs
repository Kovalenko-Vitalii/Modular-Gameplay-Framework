public interface IGameFlowOrchestrator {
    void StartNewGame(string newGameScene, string profileName) { }
    void StartLatestSaveFrom(string profileId) { }
    void StartAutoSaveFromActive() { }
    void StartSaveFromActive(string saveId) { }
    void StartManualSave(string profileId, string slotId) { }
    void ResumeGame() { }
    void GoToMainMenu() { } 
}
