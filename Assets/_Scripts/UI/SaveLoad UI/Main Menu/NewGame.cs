using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SaveSystem;
using VContainer;

/// <summary>
/// Basic script to demonstrate ability to create new save profile and run it
/// !!!
/// TEMPORARY VERSION
/// !!!
/// </summary>
public class NewGame : MonoBehaviour {
    [SerializeField] Button play;
    [SerializeField] TMP_InputField inputField;

    GameFlowController _gameFlowController;

    [Inject]
    private void Construct(GameFlowController gameFlowController){
        _gameFlowController = gameFlowController;
    }


    private void Start() => play.onClick.AddListener(OnPlayClicked);
        
    void OnPlayClicked() {
        string profileName = inputField.text;
        if (string.IsNullOrEmpty(profileName)) 
            return;

        _gameFlowController.StartNewGame("Demonstration", profileName);
    }
}
