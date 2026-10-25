using UnityEngine;
using VContainer;

[RequireComponent(typeof(AkGameObj))]
public class OccludableAudio : MonoBehaviour {
    [Header("Wwise Response")]
    [SerializeField, Range(0f, 1f)] float occlusionInfluence = 0f;

    [SerializeField] float smoothSpeed = 8f;

    public float Occlusion;

    float smoothedValue;
    AkAudioListener audioListener;

    AudioOcclusionSystem _audioOcclusionSystem;
    [Inject]
    void Construct(AudioOcclusionSystem audioOcclusionSystem) {
        _audioOcclusionSystem = audioOcclusionSystem;

        _audioOcclusionSystem.Register(this);
        audioListener = _audioOcclusionSystem.AudioListener;
    }

    private void OnDisable() {
        _audioOcclusionSystem.Unregister(this);

        if (audioListener != null)
            AkUnitySoundEngine.SetObjectObstructionAndOcclusion(gameObject, audioListener.gameObject, 0f, 0f);
    }

    private void LateUpdate() {
        if (_audioOcclusionSystem == null)
            return;

        if (audioListener == null) {
            audioListener = _audioOcclusionSystem.AudioListener;
            if (audioListener == null) return;
        }

        float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
        smoothedValue = Mathf.Lerp(smoothedValue, Occlusion, t);

        float obstruction = smoothedValue;
        float occlusion = smoothedValue * occlusionInfluence;

        AkUnitySoundEngine.SetObjectObstructionAndOcclusion(gameObject, audioListener.gameObject, obstruction, occlusion);
    }
}