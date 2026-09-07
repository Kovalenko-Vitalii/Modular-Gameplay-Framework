using SaveSystem;
using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


public class NewManualSave : MonoBehaviour {
    [SerializeField] Button button;

    SceneLoadService _sceneLoadService;
    SaveService _saveService;

    [Inject]
    void Construct(SceneLoadService sceneLoadService, SaveService saveService) {
        _sceneLoadService = sceneLoadService;
        _saveService = saveService;
    }

    void Start() {
        button.onClick.AddListener(() => _saveService.NewSave(Environment.UserName, _sceneLoadService.GetLoadedLevel()));
    }
}
