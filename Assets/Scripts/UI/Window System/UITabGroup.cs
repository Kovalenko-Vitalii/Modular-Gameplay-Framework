using System;
using UnityEngine;
using UnityEngine.UI;

public class UITabGroup : MonoBehaviour
{
    [Serializable]
    public class Tab
    {
        public string id;
        public GameObject panel;
        public Button button;
    }

    [SerializeField] private Tab[] tabs;
    [SerializeField] private string defaultTabId;

    public event Action<string> TabChanged;

    public string Current { get; private set; }

    private void OnEnable()
    {
        foreach (var tab in tabs)
        {
            if (tab.button == null) continue;
            string id = tab.id;
            tab.button.onClick.AddListener(() => ShowTab(id));
        }

        string startTab = string.IsNullOrEmpty(defaultTabId) && tabs.Length > 0
            ? tabs[0].id
            : defaultTabId;

        Current = null;
        ShowTab(startTab);
    }

    private void OnDisable()
    {
        foreach (var tab in tabs)
        {
            if (tab.button != null)
                tab.button.onClick.RemoveAllListeners();
        }
    }

    public void ShowTab(string id)
    {
        if (id == Current) return;

        foreach (var tab in tabs)
        {
            bool active = tab.id == id;
            if (tab.panel != null) tab.panel.SetActive(active);
            if (tab.button != null) tab.button.interactable = !active;
        }

        Current = id;
        TabChanged?.Invoke(id);
    }
}