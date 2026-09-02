using SaveSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadSaveSlot : MonoBehaviour {
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] GameObject rowPrefab;

    readonly List<GameObject> spawnedUISlots = new();

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
        foreach (var slot in spawnedUISlots)
            Destroy(slot);

        spawnedUISlots.Clear();

        foreach (var saveSlot in SaveService.Instance.GetAllSlotsFromActive()) {
            var instance = Instantiate(rowPrefab, scrollRect.content);
            spawnedUISlots.Add(instance);

            Action loadFunction = () => {
                GameFlowController.Instance.StartManual(SaveService.Instance.ActiveProfile.id, saveSlot.id);
            };

            Action deleteFunction = () => {
                SaveService.Instance.DeleteManualSave(saveSlot.id);
            };

            if (instance.TryGetComponent(out ProfileSlotUI slotUI))
                slotUI.Initialize(saveSlot.displayName, loadFunction, deleteFunction, "Load", "Delete");
            else
                Debug.LogWarning($"Row Prefab '{rowPrefab.name}' has no ProfileSlotUI component.");
        }
    }
}
