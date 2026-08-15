using UnityEngine;

[RequireComponent(typeof(UIWindowManager))]
public class UIScreen : MonoBehaviour
{
    [SerializeField] private UIScreenId screenId;
    [SerializeField] private GameObject root;

    private UIWindowManager windowManager;

    public string PanelId => screenId.ToString();
    public UIScreenId ScreenId => screenId;
    public UIWindowManager Windows => windowManager;

    private void Awake()
    {
        windowManager = GetComponent<UIWindowManager>();
        windowManager.Initialize(screenId);
    }

    public void Activate()
    {
        if (root != null) root.SetActive(true);
        Show();
    }

    public void Deactivate()
    {
        // Explicitly clear window state BEFORE hiding root so
        // UIWindowView.Hide() callbacks still fire correctly.
        windowManager.CloseAll();
        if (root != null) root.SetActive(false);
        Hide();
    }

    public void Show() { }
    public void Hide() { }
}