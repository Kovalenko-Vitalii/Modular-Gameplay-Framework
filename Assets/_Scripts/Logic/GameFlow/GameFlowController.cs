using SaveSystem;
using UnityEngine;
using VContainer;

/// <summary>
/// Highest point in game flow hierarchy. 
/// Designed to start global flow changes as StartGame, ExitToMenu etc.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class GameFlowController : MonoBehaviour {
    private string pendingSceneName;
    private GameMode pendingMode;
    private bool hasPendingLoad;
    private bool isNewGame = false;
    [SerializeField] string menuSceneName; // !!!

    GameStateManager _gameStateManager;
    SceneLoader _sceneLoader;
    SaveService _saveService;

    [Inject]
    void Construct(GameStateManager gameStateManager, SceneLoader sceneLoader, SaveService saveService) {
        _gameStateManager = gameStateManager;
        _sceneLoader = sceneLoader;
        _saveService = saveService;
    }

    private void Start() { 
        GoToMainMenu(); 
    }
     
    private void Awake() {
        _sceneLoader.ContentLoaded += HandleContentLoaded;
    }

    private void OnDestroy() {
        _sceneLoader.ContentLoaded -= HandleContentLoaded;
    }

    #region API
    
    public void StartNewGame(string newGameScene, string profileName) {
        if (string.IsNullOrEmpty(newGameScene)) { Debug.Log($"Invalid scene name: '{newGameScene}'"); return; }
        if (string.IsNullOrEmpty(profileName)) { Debug.Log($"Invalid profile name: '{profileName}'"); return; }

        _saveService.StartNewGame(profileName);
        RequestLoad(newGameScene, GameMode.Gameplay);
        isNewGame = true;
    }

    public void StartGame(string profileId) {  
        string sceneName = _saveService.StartLatestFrom(profileId);
        if (string.IsNullOrEmpty(sceneName)) { Debug.Log($"Invalid scene name, load canceled !"); return; }
        RequestLoad(sceneName, GameMode.Gameplay);
    }

    public void StartManual(string profileId, string slotId) {
        string sceneName = _saveService.StartFrom(profileId, slotId);
        if (string.IsNullOrEmpty(sceneName)) { Debug.Log($"Invalid scene name, load canceled !"); return; }
        RequestLoad(sceneName, GameMode.Gameplay);
    }

    public void ResumeGame() { 
        string sceneName = _saveService.StartLatestGlobal();
        if (string.IsNullOrEmpty(sceneName)) { Debug.Log($"Invalid scene name, load canceled !"); return; }
        RequestLoad(sceneName, GameMode.Gameplay);
    }

    public void GoToMainMenu() {
        _saveService.Clean();
        RequestLoad(menuSceneName, GameMode.MainMenu);
    }

    #endregion

    #region Private

    private void RequestLoad(string targetSceneName, GameMode targetMode) {
        if (_sceneLoader.IsBusy) { Debug.LogWarning($"Load of '{targetSceneName}' ignored: SceneLoader busy"); return; }

        pendingSceneName = targetSceneName;
        pendingMode = targetMode;
        hasPendingLoad = true;

        _gameStateManager.SetMode(GameMode.Loading);
        _sceneLoader.LoadContent(targetSceneName);
    }

    private void HandleContentLoaded(string loadedSceneName) {
        if (!hasPendingLoad) return;
        if (loadedSceneName != pendingSceneName) return;
            
        hasPendingLoad = false;
        _saveService.ApplyPendingData(loadedSceneName);
        _gameStateManager.SetMode(pendingMode);

        if (isNewGame) {
            _saveService.AutoSave(loadedSceneName);
            isNewGame = false;
        }
  
        Debug.Log($"Content loaded for '{loadedSceneName}', mode set to {pendingMode}");
    }

    #endregion
}