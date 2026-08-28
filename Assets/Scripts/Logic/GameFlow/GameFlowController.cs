using SaveSystem;
using UnityEngine;

/// <summary>
/// Highest point in game flow hierarchy. 
/// Designed to start global flow changes as StartGame, ExitToMenu etc.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class GameFlowController : MonoBehaviour {
    private const string TAG = "GameFlowController";
    public static GameFlowController Instance { get; private set; }

    private string pendingSceneName;
    private GameMode pendingMode;
    private bool hasPendingLoad;

    private bool isNewGame = false;
    [SerializeField] string menuSceneName; // !!! THIS IS BAD APPROACH !!!

    private void Start() => Boot();
     
    private void Awake() {
        if (Instance != null && Instance != this) { 
            Destroy(gameObject);
            return; 
        }
        
        Instance = this;
        SceneLoader.Instance.ContentLoaded += HandleContentLoaded;
        GameLog.Log(TAG, "Initialized");
    }

    private void OnDestroy() {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.ContentLoaded -= HandleContentLoaded;
    }

    public void StartNewGame(string newGameScene, string profileName) {
        if (string.IsNullOrEmpty(newGameScene)) return;
        SaveService.Instance.StartNewGame(profileName);
        RequestLoad(newGameScene, GameMode.Gameplay);
        isNewGame = true;
    }

    public void StartGame(string profileId) {  
        string sceneName = SaveService.Instance.StartLatestFrom(profileId);
        if (sceneName == null) return;
        RequestLoad(sceneName, GameMode.Gameplay);
    }

    public void StartManual(string profileId, string slotId) {
        string sceneName = SaveService.Instance.StartFrom(profileId, slotId);
        if (sceneName == null) return;
        RequestLoad(sceneName, GameMode.Gameplay);
    }

    public void ResumeGame() { 
        string sceneName = SaveService.Instance.StartLatestGlobal();
        if (sceneName == null) return;
        RequestLoad(sceneName, GameMode.Gameplay);
    }

    public void ReturnToMenu() => RequestLoad(menuSceneName, GameMode.MainMenu);

    private void Boot() => RequestLoad(menuSceneName, GameMode.MainMenu);


    private void RequestLoad(string targetSceneName, GameMode targetMode) {
        if (SceneLoader.Instance.IsBusy) {
            GameLog.Warning(TAG, $"Load of '{targetSceneName}' ignored: SceneLoader busy");
            return;
        }

        pendingSceneName = targetSceneName;
        pendingMode = targetMode;
        hasPendingLoad = true;

        GameStateManager.Instance.SetMode(GameMode.Loading);
        SceneLoader.Instance.LoadContent(targetSceneName);
    }

    private void HandleContentLoaded(string loadedSceneName) {
        if (!hasPendingLoad) return;
        if (loadedSceneName != pendingSceneName) return;
            
        hasPendingLoad = false;
        SaveService.Instance.ApplyPendingData(loadedSceneName);
        GameStateManager.Instance.SetMode(pendingMode);

        if (isNewGame) {
            SaveService.Instance.AutoSave(loadedSceneName);
            isNewGame = false;
        }
            
        
        GameLog.Log(TAG, $"Content loaded for '{loadedSceneName}', mode set to {pendingMode}");
    }
}