using SaveSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class LoadSaveProfile : MonoBehaviour {
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] GameObject rowPrefab;

    readonly List<GameObject> spawnedSlots = new();

    private GameFlowController _gameFlowController;
    SaveService _saveService;

    [Inject]
    private void Construct(GameFlowController gameFlowController, SaveService saveService) {
        _gameFlowController = gameFlowController;
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
        foreach (var slot in spawnedSlots)
            Destroy(slot);

        spawnedSlots.Clear();

        foreach (var saveProfile in _saveService.GetAllProfiles()) {
            var instance = Instantiate(rowPrefab, scrollRect.content);
            spawnedSlots.Add(instance);

            Action loadFunction = () => {
                _gameFlowController.StartGame(saveProfile.id);
            };

            Action deleteFunction = () => {
                _saveService.DeleteProfile(saveProfile.id);
            };

            if (instance.TryGetComponent(out ProfileSlotUI slotUI))
                slotUI.Initialize(saveProfile.displayName, loadFunction, deleteFunction, "Load", "Delete");
            else
                Debug.LogWarning($"profilePrefab '{rowPrefab.name}' has no ProfileSlotUI component.");
        }
    }
}
