using UnityEngine;

/// <summary>
/// Highest point in game flow hierarchy. 
/// Designed to start global flow changes as StartGame, ExitToMenu etc.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class GameFlowController : MonoBehaviour {
    private const string TAG = "GameFlowController";
    public static GameFlowController Instance { get; private set; }

    [SerializeField] string defaultSceneName;

    [SerializeField] string menuSceneName; // !!! THIS IS BAD APPROACH !!!

    private void Start() => Boot();
     
    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        GameLog.Log(TAG, "Initialized");
    }

    public void StartGame(string sceneName) => RequestLoad(sceneName, GameMode.Gameplay);
    public void ExitToMenu() => RequestLoad(menuSceneName, GameMode.MainMenu);
    private void Boot() => RequestLoad(menuSceneName, GameMode.MainMenu);

    /// <summary>
    /// Method that initiates scene load and mode change.
    /// </summary>
    /// <param name="targetSceneName"> Scene that should be loaded </param>
    /// <param name="targetMode"> Mode that should be set </param>
    private void RequestLoad(string targetSceneName, GameMode targetMode) {
        if (SceneLoader.Instance.IsBusy) {
            GameLog.Warning(TAG, $"Load of '{targetSceneName}' ignored: SceneLoader busy");
            return;
        }

        GameStateManager.Instance.SetMode(GameMode.Loading);
        SceneLoader.Instance.LoadContent(targetSceneName);

        void OnLoaded(string loadedSceneName) {
            if (loadedSceneName != targetSceneName)
                return;
            SceneLoader.Instance.ContentLoaded -= OnLoaded;
            GameStateManager.Instance.SetMode(targetMode);
            GameLog.Log(TAG, $"Content loaded for '{loadedSceneName}', mode set to {targetMode}");
        }

        SceneLoader.Instance.ContentLoaded += OnLoaded;
    }
}