using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour,
    IPointerEnterHandler {
    [SerializeField] private AK.Wwise.Event hoverEvent;
    [SerializeField] private AK.Wwise.Event clickEvent;

    private Button _button;

    private void Awake() {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(PlayClick);
    }

    private void OnDestroy() {
        _button.onClick.RemoveListener(PlayClick);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (_button.IsInteractable())
            hoverEvent.Post(gameObject);
    }

    private void PlayClick() {
        clickEvent.Post(gameObject);
    }
}