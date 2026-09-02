using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SaveSystem;

/// <summary>
/// Basic script to demonstrate ability to create new save profile and run it
/// !!!
/// TEMPORARY VERSION
/// !!!
/// </summary>
public class NewGame : MonoBehaviour {
    [SerializeField] Button play;
    [SerializeField] TMP_InputField inputField;

    private void Start() => play.onClick.AddListener(OnPlayClicked);
        
    void OnPlayClicked() {
        string profileName = inputField.text;
        if (string.IsNullOrEmpty(profileName)) 
            return;

        GameFlowController.Instance.StartNewGame("Demonstration", profileName);
    }
}
