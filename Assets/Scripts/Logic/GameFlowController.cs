using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class GameFlowController : MonoBehaviour
{
    private const string TAG = "GameFlowController";
    public static GameFlowController Instance { get; private set; }

    private string pendingScene;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() => SceneLoader.Instance.ContentLoaded += HandleContentLoaded;
    private void OnDestroy() { if (SceneLoader.Instance != null) SceneLoader.Instance.ContentLoaded -= HandleContentLoaded; }

    public void StartGame(string sceneName)
    {
        if (SceneLoader.Instance.IsBusy)
        {
            GameLog.Warning(TAG, "StartGame ignored: SceneLoader busy");
            return;
        }

        pendingScene = sceneName;
        GameStateManager.Instance.SetMode(GameMode.Loading);
        SceneLoader.Instance.LoadContent(sceneName);
    }

    private void HandleContentLoaded(string sceneName)
    {
        GameLog.Log(TAG, $"HandleContentLoaded('{sceneName}'), pending='{pendingScene}'");
        if (sceneName != pendingScene) return; // ignore unrelated loads, if any
        pendingScene = null;
        GameStateManager.Instance.SetMode(GameMode.Gameplay);
        Debug.Log($"{TAG}: Content loaded for scene '{sceneName}', game mode set to Gameplay");
    }
}