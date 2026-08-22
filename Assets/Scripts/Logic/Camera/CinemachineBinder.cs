using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Binds Cinemachine Camera to HeadPosition object in Player prefab when it`s initialized.
/// Uses tag to find HeadPosition, works when PlayerSpawned action is fired.
/// </summary>
[RequireComponent(typeof(CinemachineCamera))]
public class CinemachineBinder : MonoBehaviour {
    private const string TAG = "CinemachinePlayerTarget";

    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private string headPositionTag = "HeadPosition"; // not the best approach but didn`t find anything better ;(

    private PlayerSpawner playerSpawner;

    private void Reset() => cinemachineCamera = GetComponent<CinemachineCamera>();

    private void Awake() {
        if (cinemachineCamera == null)
            cinemachineCamera = GetComponent<CinemachineCamera>();
    }

    private void OnEnable() {
        if (playerSpawner == null)
            playerSpawner = FindAnyObjectByType<PlayerSpawner>();

        if (playerSpawner == null) {
            Debug.LogWarning($"[{TAG}] No PlayerSpawner found in scene - camera has no target");
            return;
        }

        playerSpawner.PlayerSpawned += BindCamera;

        if (playerSpawner.PlayerInstance != null) // if PlayerSpawner already spawned
            BindCamera(playerSpawner.PlayerInstance);
    }

    private void OnDisable() {
        if (playerSpawner != null)
            playerSpawner.PlayerSpawned -= BindCamera;
    }

    private void BindCamera(GameObject player) {
        Transform headPosition = FindTaggedChild(player.transform, headPositionTag);

        if (headPosition == null)
            headPosition = player.transform;

        cinemachineCamera.Follow = headPosition; 

        if (headPosition == player.transform)
            Debug.LogWarning($"[{TAG}] No child tagged '{headPositionTag}' found under '{player.name}' - LookAt falling back to root transform");

        Debug.Log($"[{TAG}] Bounded to player (HeadPosition target: '{headPosition.name}')");
    }

    /// <summary>
    /// Searches root and all its descendants for the first transform with the
    /// given tag. Returns null if the tag isn't found on anything, or if the
    /// tag itself hasn't been created in Project Settings > Tags and Layers.
    /// 
    /// +++
    /// Make this method a public function in tools section !
    /// +++
    /// </summary>
    private static Transform FindTaggedChild(Transform root, string tag)
    {
        if (string.IsNullOrEmpty(tag))
            return null;

        try {
            foreach (var candidate in root.GetComponentsInChildren<Transform>(true)) {
                if (candidate.CompareTag(tag))
                    return candidate;
            }
        }
        catch (UnityException ex) {
            Debug.LogError($"[{TAG}] Tag '{tag}' isn't defined in Project Settings > Tags and Layers: {ex.Message}");
        }

        return null;
    }
}