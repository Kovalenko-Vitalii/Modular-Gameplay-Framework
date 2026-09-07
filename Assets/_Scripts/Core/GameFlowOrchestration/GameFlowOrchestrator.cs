using Cysharp.Threading.Tasks;
using SaveSystem;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// Highest point in game flow hierarchy. 
/// Designed to start global flow changes as StartGame, ExitToMenu etc.
/// </summary>
public class GameFlowOrchestrator : IGameFlowOrchestrator, IInitializable {
    bool isNewGame = false;
    string _menuShellName;
    string _gameplayShellName;

    SceneDatabase _sceneDatabase;
    GameModeProvider _gameModeProvider;
    SceneLoadService _sceneLoadService;
    SaveService _saveService;

    [Inject]
    public GameFlowOrchestrator(SceneDatabase sceneDatabase) => _sceneDatabase = sceneDatabase;

    [Inject]
    void Construct(GameModeProvider gameModeProvider, SceneLoadService sceneLoadService, SaveService saveService) {
        _gameModeProvider = gameModeProvider;
        _sceneLoadService = sceneLoadService;
        _saveService = saveService;

        _menuShellName = _sceneDatabase.MainMenuSchell.sceneName;
        _gameplayShellName = _sceneDatabase.GameplayShell.sceneName;
    }
        
    public void Initialize() =>GoToMainMenu(); 

    #region API

    public void StartNewGame(string newGameScene, string profileName) {
        if (string.IsNullOrEmpty(newGameScene)) { Debug.Log($"Invalid scene name: '{newGameScene}'"); return; }
        if (string.IsNullOrEmpty(profileName)) { Debug.Log($"Invalid profile name: '{profileName}'"); return; }

        _saveService.StartNewGame(profileName);
        isNewGame = true;
        StartGameplay(newGameScene).Forget();
    }

    public void StartLatestSaveFrom(string profileId) {
        if (string.IsNullOrEmpty(profileId)) { Debug.Log($"Invalid profile id: '{profileId}'"); return; }

        string sceneName = _saveService.StartLatestFrom(profileId);
        if (string.IsNullOrEmpty(sceneName)) { Debug.Log("Invalid scene name, load canceled!"); return; }
        StartGameplay(sceneName).Forget();
    }

    public void StartAutoSaveFromActive() { 
        string sceneName = _saveService.StartFrom(_saveService.ActiveProfile.id, _saveService.ActiveProfile.autoSave.id);
        if (string.IsNullOrEmpty(sceneName)) { Debug.Log("Invalid scene name, load canceled!"); return; }
        StartGameplay(sceneName).Forget();
    }

    public void StartSaveFromActive(string saveId) {
        string sceneName = _saveService.StartFrom(_saveService.ActiveProfile.id, saveId);
        if (string.IsNullOrEmpty(sceneName)) { Debug.Log("Invalid scene name, load canceled!"); return; }
        StartGameplay(sceneName).Forget();
    }

    public void StartManualSave(string profileId, string slotId) {
        if (string.IsNullOrEmpty(profileId)) { Debug.Log($"Invalid profile id: '{profileId}'"); return; }
        if (string.IsNullOrEmpty(slotId)) { Debug.Log($"Invalid slot id: '{slotId}'"); return; }

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
        if (_sceneLoadService.IsBusy) { Debug.LogWarning($"Start gameplay '{levelSceneName}' ignored: busy"); return; }

        _gameModeProvider.SetMode(GameMode.Loading);

        if (!await _sceneLoadService.LoadShell(_gameplayShellName)) {
            Debug.LogError($"Failed to load gameplay shell '{_gameplayShellName}'");
            _gameModeProvider.SetMode(GameMode.MainMenu);
            return;
        }

        if (!await _sceneLoadService.LoadLevel(levelSceneName)) {
            Debug.LogError($"Failed to load level '{levelSceneName}'");
            _gameModeProvider.SetMode(GameMode.MainMenu);
            return;
        }

        _saveService.ApplyPendingData(levelSceneName);
        _gameModeProvider.SetMode(GameMode.Gameplay);

        if (isNewGame) {
            _saveService.AutoSave(levelSceneName);
            isNewGame = false;
        }
    }

    async UniTaskVoid LoadMenu() {
        _saveService.Clean();
        _gameModeProvider.SetMode(GameMode.Loading);

        if (!await _sceneLoadService.LoadShell(_menuShellName)) { Debug.LogError($"Failed to load menu '{_menuShellName}'"); return; }

        _gameModeProvider.SetMode(GameMode.MainMenu);
    }

    #endregion
}