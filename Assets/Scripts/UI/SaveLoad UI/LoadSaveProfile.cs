using SaveSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class LoadSaveProfile : MonoBehaviour {
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] GameObject profilePrefab;

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
        if (profilePrefab == null) {
            Debug.LogWarning("No profilePrefab assigned!");
            return;
        }

        Refresh();
    }

    void OnEnable() => SaveService.Instance.ProfilesChanged += Refresh;
    void OnDisable() { 
        if (SaveService.Instance != null) 
            SaveService.Instance.ProfilesChanged -= Refresh; 
    }

    void Refresh() {
        foreach (var slot in spawnedSlots)
            Destroy(slot);

        spawnedSlots.Clear();

        foreach (var saveProfile in SaveService.Instance.GetAllProfiles()) {
            var instance = Instantiate(profilePrefab, scrollRect.content);
            spawnedSlots.Add(instance);

            if (instance.TryGetComponent(out ProfileSlotUI slotUI))
                slotUI.Initialize(saveProfile);
            else
                Debug.LogWarning($"profilePrefab '{profilePrefab.name}' has no ProfileSlotUI component.");
        }
    }
}
