using UnityEngine;

public class SurfaceIdentifier : MonoBehaviour
{
    [SerializeField] private SurfaceType surfaceType = SurfaceType.Tile;

    public SurfaceType SurfaceType => surfaceType;
}

public enum SurfaceType
{
    Tile,
    Concrete,
    Wood,
    Metal,
    Grass,
    Dirt,
    Gravel,
    Water
}