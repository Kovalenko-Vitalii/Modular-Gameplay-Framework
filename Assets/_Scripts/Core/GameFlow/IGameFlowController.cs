using UnityEngine;

public interface IGameFlowController {
    void StartNewGame(string newGameScene, string profileName) { }
    void StartGame(string profileId) { }
    void StartAutoFromActive() { }
    void StartFromActive(string saveId) { }
    void StartManual(string profileId, string slotId) { }
    void ResumeGame() { }
    void GoToMainMenu() { } 
}
