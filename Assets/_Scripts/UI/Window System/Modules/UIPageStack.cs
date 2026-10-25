using System;
using System.Collections.Generic;
using UnityEngine;

public class UIPageStack : MonoBehaviour
{
    [Serializable]
    public class Page
    {
        public string id;
        public GameObject panel;
    }

    [SerializeField] private Page[] pages;
    [SerializeField] private string rootPageId;

    private readonly List<string> history = new();

    public event Action<string, string> PageChanged; // (previous, next)

    public string Current => history.Count > 0 ? history[^1] : null;
    public bool CanGoBack => history.Count > 1;

    private void OnEnable()
    {
        history.Clear();
        Show(rootPageId);
    }

    public void Open(string id)
    {
        if (id == Current) return;
        history.Add(id);
        Show(id);
    }

    public void Back()
    {
        if (!CanGoBack) return;
        history.RemoveAt(history.Count - 1);
        Show(Current);
    }

    public void Reset() // e.g. when the window closes
    {
        history.Clear();
        Show(rootPageId);
    }

    private void Show(string id)
    {
        string previous = Current == id ? null : Current;
        foreach (var page in pages)
            if (page.panel != null) page.panel.SetActive(page.id == id);

        if (history.Count == 0 || history[^1] != id) history.Add(id);
        PageChanged?.Invoke(previous, id);
    }
}