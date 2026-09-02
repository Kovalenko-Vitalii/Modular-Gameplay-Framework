using SaveSystem;
using UnityEngine;

/// <summary>
/// Highest point in game flow hierarchy. 
/// Designed to start global flow changes as StartGame, ExitToMenu etc.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class GameFlowController : MonoBehaviour, IService {
    public static GameFlowController Instance { get; private set; }

    private string pendingSceneName;
    private GameMode pendingMode;
    private bool hasPendingLoad;
    private bool isNewGame = false;
    [SerializeField] string menuSceneName; // !!!

    public void Initialize() { }

    private void Start() => GoToMainMenu();
     
    private void Awake() {
        if (Instance != null && Instance != this) { 
            Destroy(gameObject);
            return; 
        }
        
        Instance = this;
        SceneLoader.Instance.ContentLoaded += HandleContentLoaded;
        Debug.Log("Initialized");
    }

    private void OnDestroy() {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.ContentLoaded -= HandleContentLoaded;
    }

    #region API
    
    public void StartNewGame(string newGameScene, string profileName) {
        if (string.IsNullOrEmpty(newGameScene)) { Debug.Log($"Invalid scene name: '{newGameScene}'"); return; }
        if (string.IsNullOrEmpty(profileName)) { Debug.Log($"Invalid profile name: '{profileName}'"); return; }

        SaveService.Instance.StartNewGame(profileName);
        RequestLoad(newGameScene, GameMode.Gameplay);
        isNewGame = true;
    }

    public void StartGame(string profileId) {  
        string sceneName = SaveService.Instance.StartLatestFrom(profileId);
        if (string.IsNullOrEmpty(sceneName)) { Debug.Log($"Invalid scene name, load canceled !"); return; }
        RequestLoad(sceneName, GameMode.Gameplay);
    }

    public void StartManual(string profileId, string slotId) {
        string sceneName = SaveService.Instance.StartFrom(profileId, slotId);
        if (string.IsNullOrEmpty(sceneName)) { Debug.Log($"Invalid scene name, load canceled !"); return; }
        RequestLoad(sceneName, GameMode.Gameplay);
    }

    public void ResumeGame() { 
        string sceneName = SaveService.Instance.StartLatestGlobal();
        if (string.IsNullOrEmpty(sceneName)) { Debug.Log($"Invalid scene name, load canceled !"); return; }
        RequestLoad(sceneName, GameMode.Gameplay);
    }

    public void GoToMainMenu() {
        SaveService.Instance.Clean();
        RequestLoad(menuSceneName, GameMode.MainMenu);
    }

    #endregion

    #region Private

    private void RequestLoad(string targetSceneName, GameMode targetMode) {
        if (SceneLoader.Instance.IsBusy) { Debug.LogWarning($"Load of '{targetSceneName}' ignored: SceneLoader busy"); return; }

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
  
        Debug.Log($"Content loaded for '{loadedSceneName}', mode set to {pendingMode}");
    }

    #endregion
}