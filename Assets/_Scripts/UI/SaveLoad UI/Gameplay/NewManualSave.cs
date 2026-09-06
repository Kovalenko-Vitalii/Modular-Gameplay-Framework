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
        button.onClick.AddListener(() => _saveService.NewSave(Environment.UserName, _sceneLoader.GetLoadedLevel()));
    }
}
