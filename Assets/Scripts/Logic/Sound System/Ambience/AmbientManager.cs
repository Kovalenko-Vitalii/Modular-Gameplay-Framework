using System;
using System.Collections.Generic;
using UnityEngine;

public class AmbientManager : MonoBehaviour
{
    public static AmbientManager Instance { get; private set; }

    [SerializeField] private AmbientProfile defaultProfile;
    [SerializeField] private AmbientLayer[] layers;
    [SerializeField] private float fadeSpeed = 2f;
    
    Dictionary<AmbientLayerId, AmbientLayer> lookup;
    readonly List<AmbientZone> activeZones = new();
    readonly Dictionary<AmbientLayerId, float> globalState = new();
    Transform listener;

    [Header("DEBUG WEATHER")]
    [SerializeField] private float debugRain;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        listener = Camera.main.transform;
        lookup = new();

        foreach (var layer in layers)
        {
            lookup[layer.id] = layer;

            layer.source.loop = true;
            layer.source.volume = 0;

            layer.source.Play();
        }
    }

    private void Update()
    {
        SetGlobalLayer(AmbientLayerId.Rain, debugRain);
        CalculateTargets();
        ApplyFade();
    }





    // =============================
    // WEATHER API
    // =============================
    public void SetGlobalLayer( AmbientLayerId id, float value)
    {
        globalState[id] =
            Mathf.Clamp01(value);
    }

    // =============================
    // ZONES
    // =============================
    public void RegisterZone(AmbientZone zone)
    {
        if (!activeZones.Contains(zone))
            activeZones.Add(zone);
    }

    public void UnregisterZone(AmbientZone zone) => activeZones.Remove(zone);

    // =============================
    // MIXING
    // =============================
    private void CalculateTargets()
    {
        Dictionary<AmbientLayerId, float> result
            = new();

        foreach (AmbientLayerId id
            in Enum.GetValues(typeof(AmbientLayerId)))
        {
            result[id] = 0;
        }

        // Global sounds
        foreach (var pair in globalState)
        {
            result[pair.Key] = pair.Value;
        }

        // Default world ambience
        ApplyProfile(
            defaultProfile,
            1f,
            result);

        // Local zones
        foreach (var zone in activeZones)
        {
            if (zone.Profile == null)
                continue;

            float influence =
                zone.GetInfluence(listener.position);

            ApplyProfile(
                zone.Profile,
                influence,
                result);
        }

        foreach (var pair in result)
        {
            if (lookup.TryGetValue(
                pair.Key,
                out AmbientLayer layer))
            {
                layer.targetWeight =
                    pair.Value;
            }
        }
    }

    private void ApplyFade()
    {
        foreach (var layer in layers)
        {
            layer.currentWeight =
                Mathf.MoveTowards(
                    layer.currentWeight,
                    layer.targetWeight,
                    fadeSpeed *
                    Time.deltaTime);
            layer.source.volume =
                layer.currentWeight;
        }
    }

    private void ApplyProfile(AmbientProfile profile,float influence,Dictionary<AmbientLayerId, float> result)
    {
        if (profile == null)
            return;

        foreach (var local in profile.localLayers)
        {
            result[local.layer] =
                Mathf.Max(
                    result[local.layer],
                    local.volume * influence);
        }

        foreach (var limiter in profile.globalLimits)
        {
            float current =
                result[limiter.layer];


            result[limiter.layer] =
                Mathf.Min(
                    current,
                    limiter.maxVolume);
        }
    }
}





[Serializable]
public class AmbientLayer
{
    public AmbientLayerId id;

    public AudioSource source;

    [HideInInspector]
    public float currentWeight;

    [HideInInspector]
    public float targetWeight;
}