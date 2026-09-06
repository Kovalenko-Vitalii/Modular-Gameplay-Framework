using Cysharp.Threading.Tasks;
using SaveSystem;
using UnityEngine;
using VContainer;

/// <summary>
/// Highest point in game flow hierarchy. 
/// Designed to start global flow changes as StartGame, ExitToMenu etc.
/// </summary>
public class GameFlowController : MonoBehaviour {
    private bool isNewGame = false;
    [SerializeField] string menuShellName; // !!!
    [SerializeField] string gameplayShellName; // !!!

    GameStateManager _gameStateManager;
    SceneLoader _sceneLoader;
    SaveService _saveService;

    [Inject]
    void Construct(GameStateManager gameStateManager, SceneLoader sceneLoader, SaveService saveService) {
        _gameStateManager = gameStateManager;
        _sceneLoader = sceneLoader;
        _saveService = saveService;
    }

    private void Start() => GoToMainMenu();

    #region API

    public void StartNewGame(string newGameScene, string profileName) {
        if (string.IsNullOrEmpty(newGameScene)) { Debug.Log($"Invalid scene name: '{newGameScene}'"); return; }
        if (string.IsNullOrEmpty(profileName)) { Debug.Log($"Invalid profile name: '{profileName}'"); return; }

        _saveService.StartNewGame(profileName);
        isNewGame = true;
        StartGameplay(newGameScene).Forget();
    }

    public void StartGame(string profileId) {
        string sceneName = _saveService.StartLatestFrom(profileId);
        if (string.IsNullOrEmpty(sceneName)) { Debug.Log("Invalid scene name, load canceled!"); return; }
        StartGameplay(sceneName).Forget();
    }

    public void StartManual(string profileId, string slotId) {
        string sceneName = _saveService.StartFrom(profileId, slotId);
        if (string.IsNullOrEmpty(sceneName)) { Debug.Log("Invalid scene name, load canceled!"); return; }
        StartGameplay(sceneName).Forget();
    }

    public void ResumeGame() {
        string sceneName = _saveService.StartLatestGlobal();
        if (string.IsNullOrEmpty(sceneName)) { Debug.Log("Invalid scene name, load canceled!"); return; }
        StartGameplay(sceneName).Forget();
    }

    public void GoToMainMenu() => LoadMenu().Forget();

    #endregion

    #region Private

    async UniTaskVoid StartGameplay(string levelSceneName) {
        if (_sceneLoader.IsBusy) { Debug.LogWarning($"Start gameplay '{levelSceneName}' ignored: busy"); return; }

        _gameStateManager.SetMode(GameMode.Loading);

        if (!await _sceneLoader.LoadShell(gameplayShellName)) {
            Debug.LogError($"Failed to load gameplay shell '{gameplayShellName}'");
            _gameStateManager.SetMode(GameMode.MainMenu);
            return;
        }

        if (!await _sceneLoader.LoadLevel(levelSceneName)) {
            Debug.LogError($"Failed to load level '{levelSceneName}'");
            _gameStateManager.SetMode(GameMode.MainMenu);
            return;
        }

        _saveService.ApplyPendingData(levelSceneName);
        _gameStateManager.SetMode(GameMode.Gameplay);

        if (isNewGame) {
            _saveService.AutoSave(levelSceneName);
            isNewGame = false;
        }
    }


    async UniTaskVoid LoadMenu() {
        _saveService.Clean();
        _gameStateManager.SetMode(GameMode.Loading);

        if (!await _sceneLoader.LoadShell(menuShellName)) { Debug.LogError($"Failed to load menu '{menuShellName}'"); return; }

        _gameStateManager.SetMode(GameMode.MainMenu);
    }

    #endregion
}