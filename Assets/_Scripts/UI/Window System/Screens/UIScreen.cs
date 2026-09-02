using UnityEngine;

[RequireComponent(typeof(UIWindowManager))]
public class UIScreen : MonoBehaviour
{
    [SerializeField] private UIScreenId screenId;
    [SerializeField] private GameObject root;
    private UIWindowManager WindowManager => GetComponent<UIWindowManager>();

    public string PanelId => screenId.ToString();
    public UIScreenId ScreenId => screenId;
    public UIWindowManager Windows => WindowManager;

    private void Awake()
    {
        WindowManager.Initialize(screenId);
    }

    public void Activate()
    {
        if (root != null) root.SetActive(true);
        WindowManager.OpenDefaults();
    }

    public void Deactivate()
    {
        // Explicitly clear window state BEFORE hiding root so
        // UIWindowView.Hide() callbacks still fire correctly.
        WindowManager.CloseAll();
        if (root != null) root.SetActive(false);
    }
}