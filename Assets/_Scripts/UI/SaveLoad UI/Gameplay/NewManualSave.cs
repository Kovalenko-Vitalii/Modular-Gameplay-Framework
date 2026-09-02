using SaveSystem;
using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


public class NewManualSave : MonoBehaviour {
    [SerializeField] Button button;

    SceneLoader _sceneLoader;
    SaveService _saveService;

    [Inject]
    void Construct(SceneLoader sceneLoader, SaveService saveService) {
        _sceneLoader = sceneLoader;
        _saveService = saveService;
    }


    void Start() {
        button.onClick.AddListener(() => _saveService.NewSave(Environment.UserName,
        _sceneLoader.CurrentContentScene));
    }

    public static string GetTimeAgo(long updatedUtcTicks) {
        DateTime updated = new DateTime(updatedUtcTicks, DateTimeKind.Utc);
        TimeSpan elapsed = DateTime.UtcNow - updated;

        if (elapsed.TotalHours < 1)
            return $"{(int)elapsed.TotalMinutes} minutes ago";

        return $"{(int)elapsed.TotalHours} hours ago";
    }
}
