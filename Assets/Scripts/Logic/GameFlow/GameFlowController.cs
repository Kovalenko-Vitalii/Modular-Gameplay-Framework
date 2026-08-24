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
        RequestLoad(newGameScene, GameMode.Gameplay);
        SaveManager.Instance.NewProfilePreparation(newGameScene, profileName);
    }
    public void StartGame(string profileId) {
        string slotId = SaveManager.Instance.GetLatestSlotId(profileId);
        SaveSlotData latestData = SaveManager.Instance.GetSaveData(profileId, slotId);
        string sceneName = latestData.sceneName;

        RequestLoad(sceneName, GameMode.Gameplay);
        SaveManager.Instance.PutCacheData(profileId, slotId);
    }
    public void ResumeGame() { 
        var (latestProfileId, latestSlotId) = SaveManager.Instance.GetLatestSaveInfo();
        SaveSlotData latestData = SaveManager.Instance.GetSaveData(latestProfileId, latestSlotId);
        string sceneName = latestData.sceneName;

        RequestLoad(sceneName, GameMode.Gameplay);
        SaveManager.Instance.PutCacheData(latestProfileId, latestSlotId);
        Debug.Log(latestProfileId);
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
        if (!hasPendingLoad)
            return;

        if (loadedSceneName != pendingSceneName)
            return;

        hasPendingLoad = false;
        SaveManager.Instance.ApplyCacheData(loadedSceneName);
        GameStateManager.Instance.SetMode(pendingMode);
        GameLog.Log(TAG, $"Content loaded for '{loadedSceneName}', mode set to {pendingMode}");
    }
}