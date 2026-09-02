using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Ambient Profile")]
public class AmbientProfile : ScriptableObject
{
    [Header("Sounds created by this zone")]
    public LocalAmbientLayer[] localLayers;


    [Header("Maximum volume allowed for world sounds")]
    public AmbientLimiter[] globalLimits;
}



[Serializable]
public class LocalAmbientLayer
{
    public AmbientLayerId layer;


    [Range(0f, 1f)]
    public float volume = 1f;
}



[Serializable]
public class AmbientLimiter
{
    public AmbientLayerId layer;

    [Range(0f, 1f)]
    public float maxVolume = 1f;
}

public enum AmbientLayerId
{
    Default,

    // Local
    Forest,
    Indoor,
    Basement,
    Cave,
    Wind,

    // Global
    Rain
}