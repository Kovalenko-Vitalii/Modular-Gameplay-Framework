using SaveSystem;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class LoadAutoSave : MonoBehaviour
{
    [SerializeField] Button button;

    GameFlowController _gameFlowController;
    SaveService _saveService;

    [Inject]
    private void Construct(GameFlowController gameFlowController, SaveService saveService) {
        _gameFlowController = gameFlowController;
        _saveService = saveService;
    }

    void Start() {
        var acvtiveProfile = _saveService.ActiveProfile;
        button.onClick.AddListener(() => _gameFlowController.StartManual(acvtiveProfile.id, acvtiveProfile.autoSave.id));
    }
}
