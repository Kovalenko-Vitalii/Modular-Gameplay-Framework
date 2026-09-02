using SaveSystem;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class ResumeGameButton : MonoBehaviour {
    Button button;

    private void Awake() => button = GetComponent<Button>();

    GameFlowController _gameFlowController;
    SaveService _saveService;

    [Inject]
    private void Construct(GameFlowController gameFlowController, SaveService saveService) {
        _gameFlowController = gameFlowController;
        _saveService = saveService;
    }

    private void Start() {
        button.interactable = _saveService.CanResume();
        button.onClick.AddListener(OnResumeClicked);

        _saveService.ProfilesChanged += HandleSaveChanged;
    }

    private void OnDestroy() {
        if (button != null)
            button.onClick.RemoveListener(OnResumeClicked);

        _saveService.ProfilesChanged -= HandleSaveChanged;
        
    }

    private void OnResumeClicked() => _gameFlowController.ResumeGame();

    private void HandleSaveChanged() => button.interactable = _saveService.CanResume();
}
