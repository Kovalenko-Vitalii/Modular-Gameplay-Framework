using SaveSystem;
using System;
using UnityEngine;
using UnityEngine.UI;


public class NewManualSave : MonoBehaviour {
    [SerializeField] Button button;

    void Start() {
        button.onClick.AddListener(() => SaveService.Instance.NewManualSave(Environment.UserName,
            SceneLoader.Instance.CurrentContentScene));
    }

    public static string GetTimeAgo(long updatedUtcTicks) {
        DateTime updated = new DateTime(updatedUtcTicks, DateTimeKind.Utc);
        TimeSpan elapsed = DateTime.UtcNow - updated;

        if (elapsed.TotalHours < 1)
            return $"{(int)elapsed.TotalMinutes} minutes ago";

        return $"{(int)elapsed.TotalHours} hours ago";
    }
}
