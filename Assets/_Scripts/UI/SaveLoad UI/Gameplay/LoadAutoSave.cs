using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class LoadAutoSave : MonoBehaviour {
    [SerializeField] Button button;

    IGameFlowOrchestrator _gameFlowController;

    [Inject] void Construct(IGameFlowOrchestrator gameFlowController) => _gameFlowController = gameFlowController;
  
    private void OnEnable() => button.onClick.AddListener(() => _gameFlowController.StartAutoSaveFromActive());
}
