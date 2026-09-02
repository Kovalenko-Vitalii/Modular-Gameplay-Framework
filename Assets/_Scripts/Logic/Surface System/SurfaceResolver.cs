using UnityEngine;

public class SurfaceResolver : MonoBehaviour, IService
{
    public static SurfaceResolver Instance { get; private set; }
    [Header("Database")]
    [SerializeField] private SurfaceDatabase database;

    [Header("Raycast")]
    [SerializeField] private LayerMask surfaceMask = ~0;
    [SerializeField] private float rayStartHeight = 0.25f;
    [SerializeField] private float rayDistance = 1.5f;

    private SurfaceType DefaultSurface =>
        database != null ? database.DefaultSurface : SurfaceType.Tile;

    public void Initialize() { }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }   

    public SurfaceEntry GetSurfaceBelow(Vector3 worldPosition)
    {
        SurfaceType surfaceType = ResolveBelow(worldPosition);
        return GetSurface(surfaceType);
    }

    public SurfaceEntry GetSurfaceFromHit(RaycastHit hit)
    {
        SurfaceType surfaceType = ResolveFromHit(hit);
        return GetSurface(surfaceType);
    }

    public SurfaceEntry GetSurface(SurfaceType surfaceType)
    {
        if (database == null)
            return null;

        return database.GetSurface(surfaceType);
    }

    public SurfaceType ResolveBelow(Vector3 worldPosition)
    {
        Vector3 origin = worldPosition + Vector3.up * rayStartHeight;

        bool hasHit = Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit hit,
            rayDistance,
            surfaceMask,
            QueryTriggerInteraction.Ignore);

        if (!hasHit)
            return DefaultSurface;

        return ResolveFromHit(hit);
    }

    public SurfaceType ResolveFromHit(RaycastHit hit)
    {
        if (hit.collider == null)
            return DefaultSurface;

        SurfaceIdentifier identifier =
            hit.collider.GetComponentInParent<SurfaceIdentifier>();

        if (identifier != null)
            return identifier.SurfaceType;

        if (hit.collider is TerrainCollider)
            return ResolveTerrainSurface(hit);

        return DefaultSurface;
    }

    private SurfaceType ResolveTerrainSurface(RaycastHit hit)
    {
        Terrain terrain = hit.collider.GetComponent<Terrain>();

        if (terrain == null)
            terrain = hit.collider.GetComponentInParent<Terrain>();

        if (terrain == null)
            return DefaultSurface;

        TerrainData data = terrain.terrainData;

        if (data == null)
            return DefaultSurface;

        TerrainLayer[] terrainLayers = data.terrainLayers;

        if (terrainLayers == null || terrainLayers.Length == 0)
            return DefaultSurface;

        Vector3 terrainLocalPosition = hit.point - terrain.transform.position;

        int x = Mathf.RoundToInt(
            (terrainLocalPosition.x / data.size.x) * (data.alphamapWidth - 1));

        int z = Mathf.RoundToInt(
            (terrainLocalPosition.z / data.size.z) * (data.alphamapHeight - 1));

        x = Mathf.Clamp(x, 0, data.alphamapWidth - 1);
        z = Mathf.Clamp(z, 0, data.alphamapHeight - 1);

        float[,,] alpha = data.GetAlphamaps(x, z, 1, 1);

        int bestLayerIndex = 0;
        float bestValue = 0f;

        int layerCount = alpha.GetLength(2);

        for (int i = 0; i < layerCount; i++)
        {
            float value = alpha[0, 0, i];

            if (value > bestValue)
            {
                bestValue = value;
                bestLayerIndex = i;
            }
        }

        if (bestLayerIndex < 0 || bestLayerIndex >= terrainLayers.Length)
            return DefaultSurface;

        TerrainLayer terrainLayer = terrainLayers[bestLayerIndex];

        if (database == null)
            return DefaultSurface;

        return database.GetSurfaceType(terrainLayer);
    }
}