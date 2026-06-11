using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all ambient audio layers.
///
/// Mental model
/// ────────────
/// Global state  = what the world is doing right now (rain, wind, time of day).
/// Zone profiles = how a location *filters* those globals and what local sounds
///                 it adds.
///
/// Rain / Wind pipeline
/// ────────────────────
/// 1. You call SetRain(0.8f)  →  globalRain = 0.8
/// 2. The cave profile has rainMultiplier = 0.2
///    → rain heard inside cave = 0.8 × 0.2 = 0.16
/// 3. Outside (no zone, or a zone with rainMultiplier = 1)
///    → rain heard            = 0.8 × 1.0 = 0.8
///
/// If globalRain == 0, no zone can ever produce rain sound,
/// because anything × 0 = 0.
/// </summary>
public class AmbientManager : MonoBehaviour
{
    public static AmbientManager Instance { get; private set; }

    [Header("Layers")]
    [SerializeField] private AmbientLayer[] layers;

    [Header("Smoothing")]
    [SerializeField] private float fadeSpeed = 2f;

    // ── Global weather state ──────────────────────────────────────────────────
    // These represent what the weather is actually doing in the world.
    // They are NOT audio volumes; they are inputs to the mixing pipeline.
    [Header("Global Weather (0 = none, 1 = full)")]
    [Range(0f, 1f)] public float globalRain;
    [Range(0f, 1f)] public float globalWind;

    // ── Time of day (0 = full day, 1 = full night) ───────────────────────────
    [Header("Time of Day (0 = day, 1 = night)")]
    [Range(0f, 1f)] public float timeOfDay;

    // ── Internals ─────────────────────────────────────────────────────────────
    private readonly List<AmbientZone> activeZones = new();
    private Dictionary<AmbientLayerId, AmbientLayer> layerLookup;
    private Transform listener;

    // ─────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        listener = Camera.main != null ? Camera.main.transform : null;

        layerLookup = new Dictionary<AmbientLayerId, AmbientLayer>();
        foreach (var layer in layers)
        {
            layerLookup[layer.id] = layer;
            layer.currentWeight = 0f;
            layer.targetWeight = 0f;
            layer.source.loop = true;
            layer.source.volume = 0f;
            if (!layer.source.isPlaying)
                layer.source.Play();
        }
    }

    private void Update()
    {
        if (listener == null) return;
        RecalculateTargets();
        ApplySmoothing();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────────────────────────────────

    public void SetRain(float value) => globalRain = Mathf.Clamp01(value);
    public void SetWind(float value) => globalWind = Mathf.Clamp01(value);
    public void SetTimeOfDay(float value) => timeOfDay = Mathf.Clamp01(value);

    public void RegisterZone(AmbientZone zone)
    {
        if (!activeZones.Contains(zone))
            activeZones.Add(zone);
    }

    public void UnregisterZone(AmbientZone zone) => activeZones.Remove(zone);

    // ─────────────────────────────────────────────────────────────────────────
    // Mixing
    // ─────────────────────────────────────────────────────────────────────────

    private void RecalculateTargets()
    {
        // Start with zeroed targets.
        var result = new Dictionary<AmbientLayerId, float>();
        foreach (AmbientLayerId id in Enum.GetValues(typeof(AmbientLayerId)))
            result[id] = 0f;

        // ── Base day / night (always present, no zone needed) ────────────────
        result[AmbientLayerId.BaseDay] = 1f - timeOfDay;
        result[AmbientLayerId.BaseNight] = timeOfDay;

        if (activeZones.Count == 0)
        {
            // Outdoors with no zone: weather is fully exposed.
            result[AmbientLayerId.Rain] = globalRain;
            result[AmbientLayerId.Wind] = globalWind;
        }
        else
        {
            // ── Accumulate zone contributions ────────────────────────────────
            // We track the maximum rain/wind multiplier across all active zones
            // (weighted by zone influence) so the most "open" zone wins.
            float rainMul = 0f;
            float windMul = 0f;

            foreach (var zone in activeZones)
            {
                if (zone.Profile == null) continue;

                float influence = zone.GetInfluence(listener.position);

                // Local layers (cave drips, room tone, etc.)
                foreach (var entry in zone.Profile.localLayers)
                {
                    float weighted = entry.weight * influence;
                    result[entry.layer] = Mathf.Max(result[entry.layer], weighted);
                }

                // Accumulate weather multipliers (max = most exposed zone wins)
                rainMul = Mathf.Max(rainMul, zone.Profile.rainMultiplier * influence);
                windMul = Mathf.Max(windMul, zone.Profile.windMultiplier * influence);
            }

            // Apply global weather through the accumulated multipliers.
            // If globalRain == 0 this will always be 0, regardless of profile values.
            result[AmbientLayerId.Rain] = globalRain * rainMul;
            result[AmbientLayerId.Wind] = globalWind * windMul;
        }

        // Push computed targets into layers.
        foreach (var pair in result)
        {
            if (layerLookup.TryGetValue(pair.Key, out var layer))
                layer.targetWeight = pair.Value;
        }
    }

    private void ApplySmoothing()
    {
        foreach (var layer in layers)
        {
            layer.currentWeight = Mathf.MoveTowards(
                layer.currentWeight,
                layer.targetWeight,
                fadeSpeed * Time.deltaTime
            );
            layer.source.volume = layer.currentWeight;
        }
    }
}

[Serializable]
public class AmbientLayer
{
    public AmbientLayerId id;
    public AudioSource source;

    [HideInInspector] public float currentWeight;
    [HideInInspector] public float targetWeight;
}