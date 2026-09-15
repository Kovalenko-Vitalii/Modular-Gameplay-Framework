using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Surfaces/Surface Database")]
public class SurfaceDatabase : ScriptableObject
{
    [Header("Default")]
    [SerializeField] private SurfaceType defaultSurface = SurfaceType.Grass;

    [Header("Surfaces")]
    [SerializeField] private SurfaceEntry[] surfaces; // !!! move it to a dictionary !!!

    [Header("Terrain")]
    [SerializeField] private TerrainLayerSurface[] terrainLayerSurfaces;

    public SurfaceType DefaultSurface => defaultSurface;

    public SurfaceEntry GetSurface(SurfaceType surfaceType) {
        if (surfaces == null || surfaces.Length == 0)
            return null;

        for (int i = 0; i < surfaces.Length; i++) {
            SurfaceEntry entry = surfaces[i];

            if (entry != null && entry.SurfaceType == surfaceType)
                return entry;
        }

        for (int i = 0; i < surfaces.Length; i++) {
            SurfaceEntry entry = surfaces[i];

            if (entry != null && entry.SurfaceType == defaultSurface)
                return entry;
        }

        return null;
    }

    public SurfaceType GetSurfaceType(TerrainLayer terrainLayer) {
        if (terrainLayer == null) return defaultSurface;
        if (terrainLayerSurfaces == null || terrainLayerSurfaces.Length == 0) return defaultSurface;

        for (int i = 0; i < terrainLayerSurfaces.Length; i++)  {
            TerrainLayerSurface entry = terrainLayerSurfaces[i];

            if (entry != null && entry.Layer == terrainLayer)
                return entry.SurfaceType;
        }

        return defaultSurface;
    }
}

// Class that likns terrain layer to surface type
[Serializable]
public class TerrainLayerSurface {
    [SerializeField] TerrainLayer layer;
    [SerializeField] SurfaceType surfaceType = SurfaceType.Tile;

    public TerrainLayer Layer => layer;
    public SurfaceType SurfaceType => surfaceType;
}

[Serializable]
public class SurfaceEntry {
    [SerializeField] private SurfaceType surfaceType = SurfaceType.Tile;

    [Header("Impact VFX")]
    [SerializeField] private GameObject impactParticlePrefab; 

    [Header("Decals")]
    [SerializeField] private GameObject decalPrefab; // template

    public SurfaceType SurfaceType => surfaceType;
    public GameObject ImpactParticlePrefab => impactParticlePrefab;
    public GameObject DecalPrefab => decalPrefab;
}