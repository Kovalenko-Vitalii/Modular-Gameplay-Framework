using UnityEngine;
using UnityEngine.Events;

public class UIWindowView : MonoBehaviour
{
    [SerializeField] private UIWindowDefinition definition;
    [SerializeField] private GameObject root;

    [SerializeField] private UnityEvent onShown;
    [SerializeField] private UnityEvent onHidden;

    private UIWindowManager manager;

    public string PanelId => definition != null ? definition.Id : null;
    public UIWindowDefinition Definition => definition;

    protected virtual void Awake()
    {
        manager = GetComponentInParent<UIWindowManager>();
        if (manager == null)
            GameLog.Log("UIWindowView", $"{name} has no UIWindowManager in its parent hierarchy");
    }

    protected virtual void OnEnable()
    {
        if (manager == null) return;

        manager.WindowOpened += HandleWindowOpened;
        manager.WindowClosed += HandleWindowClosed;

        SetVisible(manager.IsOpen(definition));
    }

    protected virtual void OnDisable()
    {
        if (manager == null) return;

        manager.WindowOpened -= HandleWindowOpened;
        manager.WindowClosed -= HandleWindowClosed;
    }

    private void HandleWindowOpened(UIWindowDefinition window)
    {
        if (window == definition) SetVisible(true);
    }

    private void HandleWindowClosed(UIWindowDefinition window)
    {
        if (window == definition) SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (root != null) root.SetActive(visible);
        if (visible) Show(); else Hide();
    }

    public virtual void Show() => onShown?.Invoke();
    public virtual void Hide() => onHidden?.Invoke();
}