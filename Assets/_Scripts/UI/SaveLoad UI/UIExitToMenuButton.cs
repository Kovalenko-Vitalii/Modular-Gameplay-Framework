using UnityEngine;
using UnityEngine.UI;

public class UIExitToMenuButton : MonoBehaviour {
    Button button;
    private void Awake() {
        button = GetComponent<Button>();

        if (button == null)    
            Debug.LogWarning("UIExitToMenuButton requires a Button component.", this);
    }

    private void OnEnable() {
        if (button != null)
            button.onClick.AddListener(OnExitToMenuClicked);
    }

    private void OnDisable() {
        if (button != null)
            button.onClick.RemoveListener(OnExitToMenuClicked);
    }

    private void OnExitToMenuClicked() {
        GameFlowController.Instance.GoToMainMenu();
    }
}
