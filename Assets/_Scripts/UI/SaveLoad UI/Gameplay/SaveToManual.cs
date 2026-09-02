using SaveSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SaveToManual : MonoBehaviour {
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] GameObject rowPrefab;

    readonly List<GameObject> spawnedUISlots = new();

    SceneLoader _sceneLoader;
    SaveService _saveService;

    [Inject]
    void Construct(SceneLoader sceneLoader, SaveService saveService) {
        _sceneLoader = sceneLoader;
        _saveService = saveService;
    }

    void Start() {
        if (scrollRect == null || scrollRect.content == null) Debug.LogWarning("No ScrollRect (or its Content) assigned!");
        if (rowPrefab == null) Debug.LogWarning("No profilePrefab assigned!");

        Refresh();
    }

    void Awake() {
        _saveService.ProfilesChanged += Refresh;
    }
    void OnDestroy() {
        _saveService.ProfilesChanged -= Refresh;
    }

    void Refresh() {
        foreach (var slot in spawnedUISlots)
            Destroy(slot);

        spawnedUISlots.Clear();

        foreach (var saveSlot in _saveService.GetAllSlotsFromActive()) {
            var instance = Instantiate(rowPrefab, scrollRect.content);
            spawnedUISlots.Add(instance);

            Action saveFunction = () => {
                _saveService.OverwriteSave(saveSlot.id, _sceneLoader.CurrentContentScene, saveSlot.displayName);
            };

            Action deleteFunction = () => {
                _saveService.DeleteManualSave(saveSlot.id);
            };

            if (instance.TryGetComponent(out ProfileSlotUI slotUI))
                slotUI.Initialize(saveSlot.displayName, saveFunction, deleteFunction, "Save", "Delete");
            else
                Debug.LogWarning($"Row Prefab '{rowPrefab.name}' has no ProfileSlotUI component.");
        }
    }
}
