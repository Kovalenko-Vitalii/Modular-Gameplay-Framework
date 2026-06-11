using System;
using UnityEngine;

/// <summary>
/// Defines how a location sounds.
/// 
/// Local layers play at a fixed weight when the player is in this zone
/// (e.g. cave drips, indoor room tone).
/// 
/// Weather multipliers scale the global weather values for this location
/// (e.g. a cave hears rain at 20% of its real intensity; a house hears
/// wind at 10%). A multiplier of 1 = fully exposed; 0 = completely blocked.
/// </summary>
[CreateAssetMenu(menuName = "Audio/Ambient Profile")]
public class AmbientProfile : ScriptableObject
{
    [Header("Local Layers")]
    [Tooltip("Sounds that play at a fixed weight in this zone, regardless of weather.")]
    public AmbientLayerWeight[] localLayers;

    [Header("Weather Multipliers")]
    [Tooltip("How much of the global rain intensity bleeds into this zone. " +
             "0 = fully sheltered, 1 = fully exposed.")]
    [Range(0f, 1f)] public float rainMultiplier = 1f;

    [Tooltip("How much of the global wind intensity bleeds into this zone.")]
    [Range(0f, 1f)] public float windMultiplier = 1f;
}

[Serializable]
public class AmbientLayerWeight
{
    public AmbientLayerId layer;

    [Range(0f, 1f)]
    public float weight;
}

public enum AmbientLayerId
{
    BaseDay,
    BaseNight,
    Wind,
    Rain,
    Indoor,
    Cave
}