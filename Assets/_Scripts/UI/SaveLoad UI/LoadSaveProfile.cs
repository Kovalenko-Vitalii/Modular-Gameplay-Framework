using SaveSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadSaveProfile : MonoBehaviour {
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] GameObject rowPrefab;

    readonly List<GameObject> spawnedSlots = new();

    void Start() {
        if (SaveService.Instance == null) {
            Debug.LogError("Could not link to SaveManager!");
            return;
        }

        if (scrollRect == null || scrollRect.content == null) {
            Debug.LogWarning("No ScrollRect (or its Content) assigned!");
            return;
        }

        if (rowPrefab == null) {
            Debug.LogWarning("No profilePrefab assigned!");
            return;
        }

        Refresh();
    }

    void Awake() {
        if (SaveService.Instance != null)
            SaveService.Instance.ProfilesChanged += Refresh;
    }
    void OnDestroy() { 
        if (SaveService.Instance != null) 
            SaveService.Instance.ProfilesChanged -= Refresh; 
    }

    void Refresh() {
        foreach (var slot in spawnedSlots)
            Destroy(slot);

        spawnedSlots.Clear();

        foreach (var saveProfile in SaveService.Instance.GetAllProfiles()) {
            var instance = Instantiate(rowPrefab, scrollRect.content);
            spawnedSlots.Add(instance);

            Action loadFunction = () => {
                GameFlowController.Instance.StartGame(saveProfile.id);
            };

            Action deleteFunction = () => {
                SaveService.Instance.DeleteProfile(saveProfile.id);
            };

            if (instance.TryGetComponent(out ProfileSlotUI slotUI))
                slotUI.Initialize(saveProfile.displayName, loadFunction, deleteFunction, "Load", "Delete");
            else
                Debug.LogWarning($"profilePrefab '{rowPrefab.name}' has no ProfileSlotUI component.");
        }
    }
}
