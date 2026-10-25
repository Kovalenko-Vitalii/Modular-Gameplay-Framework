using UnityEngine;

public class SurfaceIdentifier : MonoBehaviour {
    [SerializeField] SurfaceType surfaceType = SurfaceType.Grass;

    public SurfaceType SurfaceType => surfaceType;
}

public enum SurfaceType {
    DirtyGround,
    Grass,
    Gravel,
    Leaves,
    Metal,
    Mud,
    Rock,
    Sand,
    Snow,
    Tile,
    Water,
    Wood
}