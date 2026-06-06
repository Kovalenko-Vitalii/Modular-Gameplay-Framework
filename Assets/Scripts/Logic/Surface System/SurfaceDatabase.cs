using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Surfaces/Surface Database")]
public class SurfaceDatabase : ScriptableObject
{
    [Header("Fallback")]
    [SerializeField] private SurfaceType defaultSurface = SurfaceType.Tile;

    [Header("Surfaces")]
    [SerializeField] private SurfaceEntry[] surfaces;

    [Header("Terrain")]
    [SerializeField] private TerrainLayerSurface[] terrainLayerSurfaces;

    public SurfaceType DefaultSurface => defaultSurface;

    public SurfaceEntry GetSurface(SurfaceType surfaceType)
    {
        if (surfaces == null || surfaces.Length == 0)
            return null;

        for (int i = 0; i < surfaces.Length; i++)
        {
            SurfaceEntry entry = surfaces[i];

            if (entry != null && entry.SurfaceType == surfaceType)
                return entry;
        }

        for (int i = 0; i < surfaces.Length; i++)
        {
            SurfaceEntry entry = surfaces[i];

            if (entry != null && entry.SurfaceType == defaultSurface)
                return entry;
        }

        return null;
    }

    public SurfaceType GetSurfaceType(TerrainLayer terrainLayer)
    {
        if (terrainLayer == null)
            return defaultSurface;

        if (terrainLayerSurfaces == null || terrainLayerSurfaces.Length == 0)
            return defaultSurface;

        for (int i = 0; i < terrainLayerSurfaces.Length; i++)
        {
            TerrainLayerSurface entry = terrainLayerSurfaces[i];

            if (entry != null && entry.Layer == terrainLayer)
                return entry.SurfaceType;
        }

        return defaultSurface;
    }
}

// Class that likns terrain layer to surface type
[Serializable]
public class TerrainLayerSurface
{
    [SerializeField] TerrainLayer layer;
    [SerializeField] SurfaceType surfaceType = SurfaceType.Tile;

    public TerrainLayer Layer => layer;
    public SurfaceType SurfaceType => surfaceType;
}

[Serializable]
public class SurfaceEntry
{
    [SerializeField] private SurfaceType surfaceType = SurfaceType.Tile;

    [Header("Footsteps")]
    [SerializeField] private AudioClipBank walkFootsteps;
    [SerializeField] private AudioClipBank sprintFootsteps;

    [Header("Jump / Land")]
    [SerializeField] private AudioClipBank jumpStartClips;
    [SerializeField] private AudioClipBank landClips;

    [Header("Impacts")]
    [SerializeField] private AudioClipBank impactClips;

    [Header("Impact VFX")]
    [SerializeField] private GameObject impactParticlePrefab;

    [Header("Decals")]
    [SerializeField] private GameObject decalPrefab;

    public SurfaceType SurfaceType => surfaceType;
    public GameObject ImpactParticlePrefab => impactParticlePrefab;
    public GameObject DecalPrefab => decalPrefab;

    public AudioClip GetWalkFootstepClip(AudioClip previousClip = null)
    {
        return walkFootsteps.GetRandom(previousClip);
    }

    public AudioClip GetSprintFootstepClip(AudioClip previousClip = null)
    {
        if (sprintFootsteps.HasClips)
            return sprintFootsteps.GetRandom(previousClip);

        return walkFootsteps.GetRandom(previousClip);
    }

    public AudioClip GetJumpStartClip(AudioClip previousClip = null)
    {
        if (jumpStartClips.HasClips)
            return jumpStartClips.GetRandom(previousClip);

        return walkFootsteps.GetRandom(previousClip);
    }

    public AudioClip GetLandClip(AudioClip previousClip = null)
    {
        if (landClips.HasClips)
            return landClips.GetRandom(previousClip);

        return walkFootsteps.GetRandom(previousClip);
    }

    public AudioClip GetImpactClip(AudioClip previousClip = null)
    {
        return impactClips.GetRandom(previousClip);
    }
}