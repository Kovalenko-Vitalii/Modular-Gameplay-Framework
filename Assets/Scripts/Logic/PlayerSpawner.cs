using SaveSystem;
using System;
using UnityEngine;

/// <summary>
/// Spawns the player prefab into the scene and is the sole authority on where
/// !!!
/// PROTOTYPE VERSION
/// !!!
/// </summary>
public class PlayerSpawner : SaveableBehaviour
{
    private const string TAG = "PlayerSpawner";

    [SerializeField] private string id = "PlayerSpawner";
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform defaultSpawnPoint; // temporary, ill figure out later how to do this properly

    public override string saveId => id;

    public GameObject PlayerInstance { get; private set; }

    /// <summary>
    /// Action fired when player is spawned.Subscribers should not assume this will be fired on the main thread.
    /// </summary>
    public event Action<GameObject> PlayerSpawned;

    protected override void Awake()
    {
        base.Awake(); // registers this with SaveRegistry
        SpawnDefault();
    }

    /// <summary>
    /// Instantiates the player prefab at the default spawn pose if it hasn't
    /// been spawned yet.
    /// </summary>
    private void SpawnDefault()
    {
        if (PlayerInstance != null)
            return;

        if (playerPrefab == null) {
            Debug.LogError($"{TAG} No playerPrefab assigned on '{name}'.");
            return;
        }

        var (pos, rot) = DefaultPose();
        PlayerInstance = Instantiate(playerPrefab, pos, rot); // spawning player at def pose, but it will be overwritten by RestoreState() if needed
        PlayerSpawned?.Invoke(PlayerInstance);
    }

    private (Vector3 pos, Quaternion rot) DefaultPose() {
        if (defaultSpawnPoint != null) {
            return (
                defaultSpawnPoint.position,
                defaultSpawnPoint.rotation
            );
        }

        return (
            transform.position,
            transform.rotation
        );
    }

    public override object CaptureState() {
        if (PlayerInstance == null)
            return null;

        return new PlayerPositionState {
            position = PlayerInstance.transform.position,
            rotation = PlayerInstance.transform.rotation
        };
    }

    public override void RestoreState(object state) {
        if (state is not PlayerPositionState playerState) {
            Debug.LogWarning($"{TAG} RestoreState got unexpected type '{state?.GetType().Name}'.");
            return;
        }

        PlayerInstance.transform.SetPositionAndRotation(playerState.position, playerState.rotation);
    }

    public override void ResetToDefaultState() => SpawnDefault();  
}

/// <summary>
/// Saved state for tracking player position. 
/// </summary>
[Serializable]
[SaveState("PlayerPosition")]
public class PlayerPositionState
{
    public Vector3 position;
    public Quaternion rotation;
}