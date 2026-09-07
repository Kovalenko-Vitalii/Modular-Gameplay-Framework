using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class LoadAutoSave : MonoBehaviour {
    [SerializeField] Button button;

    IGameFlowController _gameFlowController;

    [Inject] void Construct(IGameFlowController gameFlowController) => _gameFlowController = gameFlowController;
  
    private void OnEnable() => button.onClick.AddListener(() => _gameFlowController.StartAutoSaveFromActive());
}
