using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter))]
public class OccludableAudio : MonoBehaviour
{
    [Header("Occlusion Response")]
    [SerializeField, Range(0f, 1f)] private float occludedVolumeMultiplier = 0.3f;

    [SerializeField] private float openCutoffFrequency = 22000f;
    [SerializeField] private float occludedCutoffFrequency = 1200f;

    [SerializeField] private float smoothSpeed = 8f;

    [HideInInspector] public float Occlusion;

    public AudioSource Source { get; private set; }
    public AudioLowPassFilter LowPass { get; private set; }

    private float baseVolume;

    private void Awake()
    {
        Source = GetComponent<AudioSource>();
        LowPass = GetComponent<AudioLowPassFilter>();
        baseVolume = Source.volume;
    }

    private void OnEnable() => AudioOcclusionSystem.Instance?.Register(this);       
    private void OnDisable() => AudioOcclusionSystem.Instance?.Unregister(this);   
    
    private void FixedUpdate()
    {
        float targetVolume = baseVolume * Mathf.Lerp(1f, occludedVolumeMultiplier, Occlusion);
        float targetCutoff = Mathf.Lerp(openCutoffFrequency, occludedCutoffFrequency, Occlusion);

        float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);

        Source.volume = Mathf.Lerp(Source.volume, targetVolume, t);
        LowPass.cutoffFrequency = Mathf.Lerp(LowPass.cutoffFrequency, targetCutoff, t);
    }

    public void RefreshBaseVolume() => baseVolume = Source.volume;
}