using UnityEngine;
using UnityEngine.Events;

public class UIWindowView : MonoBehaviour
{
    [SerializeField] private UIWindowId windowId;
    [SerializeField] private GameObject root;

    [SerializeField] private UnityEvent onShown;
    public UIWindowId WindowId => windowId;

    protected virtual void OnEnable()
    {
        UIWindowManager.WindowChanged += HandleWindowChanged;

        bool isCurrent = UIWindowManager.Instance != null &&
                          UIWindowManager.Instance.Current == windowId;
        SetVisible(isCurrent);
    }

    protected virtual void OnDisable()
    {
        UIWindowManager.WindowChanged -= HandleWindowChanged;
    }

    private void HandleWindowChanged(UIWindowId newWindow)
    {
        SetVisible(newWindow == windowId);
    }

    private void SetVisible(bool visible)
    {
        if (root != null)
            root.SetActive(visible);

        if (visible)
            OnShown();
        else
            OnHidden();
    }

    protected virtual void OnShown() 
    {
        onShown?.Invoke();
    }
    protected virtual void OnHidden() { }
}