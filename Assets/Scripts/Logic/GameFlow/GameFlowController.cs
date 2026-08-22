using UnityEngine;

// <summary>
// Mystery class that controls the flow of the game
// </summary>
[DefaultExecutionOrder(-1000)]
public class GameFlowController : MonoBehaviour {
    private const string TAG = "GameFlowController";
    public static GameFlowController Instance { get; private set; }

    [SerializeField] string menuSceneName;
    private string pendingScene;
    private GameMode pendingMode;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        GameLog.Log(TAG, "Initialized");
    }

    private void Start() {
        SceneLoader.Instance.ContentLoaded += HandleContentLoaded;
        ExitToMenu();
    }
    private void OnDestroy() { 
        if (SceneLoader.Instance != null) 
            SceneLoader.Instance.ContentLoaded -= HandleContentLoaded; 
    }

    public void StartGame(string sceneName) {
        if (SceneLoader.Instance.IsBusy) {
            GameLog.Warning(TAG, "StartGame ignored: SceneLoader busy");
            return;
        }

        pendingScene = sceneName;
        pendingMode = GameMode.Gameplay;
        GameStateManager.Instance.SetMode(GameMode.Loading);
        SceneLoader.Instance.LoadContent(sceneName);
    }

    public void ExitToMenu() {
        if (SceneLoader.Instance.IsBusy) {
            GameLog.Warning(TAG, "ExitToMenu ignored: SceneLoader busy");
            return;
        }

        pendingScene = menuSceneName;
        pendingMode = GameMode.MainMenu;
        GameStateManager.Instance.SetMode(GameMode.Loading);
        SceneLoader.Instance.LoadContent(menuSceneName);
    }

    private void HandleContentLoaded(string sceneName) {
        if (sceneName != pendingScene) 
            return; // ignore unrelated loads, if any

        pendingScene = null;
        GameStateManager.Instance.SetMode(pendingMode);
        Debug.Log($"{TAG}: Content loaded for scene '{sceneName}', game mode set to Gameplay");
    }
}