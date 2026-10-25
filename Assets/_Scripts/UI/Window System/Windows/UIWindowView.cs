using UnityEngine;
using UnityEngine.Events;
using VContainer;

public class UIWindowView : MonoBehaviour {
    [SerializeField] UIWindowDefinition _definition;
    [SerializeField] GameObject _root;

    [SerializeField] UnityEvent _onShown;
    [SerializeField] UnityEvent _onHidden;

    [SerializeField] UIWindowManager _uiWindowManager;

    public UIWindowDefinition Definition => _definition;

    protected virtual void OnEnable() {
        _uiWindowManager.TriggerOpen += HandleWindowOpened;
        _uiWindowManager.TriggerClose += HandleWindowClosed;

        SetVisible(_uiWindowManager.IsOpen(_definition));
    }

    protected virtual void OnDisable() {
        _uiWindowManager.TriggerOpen -= HandleWindowOpened;
        _uiWindowManager.TriggerClose -= HandleWindowClosed;
    }

    private void HandleWindowOpened(UIWindowDefinition window) {
        if (window == _definition) SetVisible(true);
    }

    private void HandleWindowClosed(UIWindowDefinition window) {
        if (window == _definition) SetVisible(false);
    }

    private void SetVisible(bool visible) {
        if (_root != null) _root.SetActive(visible);

        if (visible) 
            Show(); 
        else 
            Hide();
    }

    public virtual void Show() => _onShown?.Invoke();
    public virtual void Hide() => _onHidden?.Invoke();
}