using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class AudioOcclusionSystem : MonoBehaviour {
    [Header("Detection")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float updateInterval = 0.1f;
    [SerializeField] private float maxOcclusionCheckDistance = 30f;

    [Header("Performance")]
    [SerializeField] private int maxChecksPerTick = 8;

    [Header("Soft Occlusion (optional)")]
    [SerializeField] private bool useSoftOcclusion = true;
    [SerializeField, Range(1, 5)] private int softOcclusionRays = 3;
    [SerializeField] private float softOcclusionSampleRadius = 0.4f;


    private readonly List<OccludableAudio> sources = new();
    private Transform listener;
    private float timer;
    private int cursor;

    public AkAudioListener AudioListener { get; private set; }

    SurfaceResolver _surfaceResolver;

    [Inject]
    void Construct(SurfaceResolver surfaceResolver, AkAudioListener audioListener) {
        _surfaceResolver = surfaceResolver;
        AudioListener = audioListener;
        listener = audioListener.transform;
    }

    public void Register(OccludableAudio source) {
        if (!sources.Contains(source))
            sources.Add(source);
    }

    public void Unregister(OccludableAudio source) {
        int index = sources.IndexOf(source);
        if (index < 0) return;

        sources.RemoveAt(index);
        if (cursor > index) cursor--;
    }


    private float GetOcclusionStrength(OcclusionPower power) {
        return power switch {
            OcclusionPower.Low => 0.25f,
            OcclusionPower.Medium => 0.55f,
            OcclusionPower.Hard => 0.85f,
            _ => 0f
        };
    }

    private void Update() {
        if (listener == null || sources.Count == 0)
            return;

        timer += Time.deltaTime;
        if (timer < updateInterval)
            return;
        timer = 0f;

        int checksThisTick = Mathf.Min(maxChecksPerTick, sources.Count);
        for (int i = 0; i < checksThisTick; i++) {
            cursor %= sources.Count;
            UpdateOcclusion(sources[cursor]);
            cursor++;
        }
    }

    private void UpdateOcclusion(OccludableAudio source) {
        Vector3 sourcePos = source.transform.position;
        Vector3 toListener = listener.position - sourcePos;
        float distance = toListener.magnitude;

        if (distance > maxOcclusionCheckDistance) {
            source.Occlusion = 0f;
            return;
        }

        Vector3 dir = toListener / distance;

        bool blocked = Physics.Raycast(
            sourcePos,
            dir,
            out RaycastHit hit,
            distance,
            obstacleMask,
            QueryTriggerInteraction.Ignore);

        Debug.DrawLine(
            sourcePos,
            listener.position,
            blocked ? Color.red : Color.green,
            updateInterval);

        if (blocked) {
            SurfaceEntry surface = _surfaceResolver.GetSurfaceFromHit(hit);

            Debug.Log(
                $"[Occlusion] BLOCKED by: {hit.collider.name} | " +
                $"Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)} | " +
                $"Surface: {(surface != null ? surface.SurfaceType.ToString() : "NULL")}"
            );

            source.Occlusion = surface != null
                ? GetOcclusionStrength(surface.OcclusionPower)
                : 0f;
        } else {
            Debug.Log("[Occlusion] NOT BLOCKED");

            source.Occlusion = 0f;
        }
    }

    private float SampleSoftOcclusion(
    Vector3 sourcePos,
    Vector3 dir,
    float distance) {

        Vector3 right = Vector3.Cross(dir, Vector3.up);

        if (right.sqrMagnitude < 0.0001f)
            right = Vector3.Cross(dir, Vector3.forward);

        right = right.normalized * softOcclusionSampleRadius;

        Vector3 up =
            Vector3.Cross(right.normalized, dir).normalized *
            softOcclusionSampleRadius;

        float totalOcclusion = 0f;

        for (int i = 0; i < softOcclusionRays; i++) {

            Vector3 offset = i switch {
                0 => Vector3.zero,
                1 => right,
                2 => -right,
                3 => up,
                _ => -up
            };

            Vector3 origin = sourcePos + offset;
            Vector3 target = listener.position + offset;

            Vector3 delta = target - origin;
            float dist = delta.magnitude;

            if (Physics.Raycast(
                origin,
                delta / Mathf.Max(dist, 0.0001f),
                out RaycastHit hit,
                dist,
                obstacleMask,
                QueryTriggerInteraction.Ignore)) {

                SurfaceEntry surface =
                    _surfaceResolver.GetSurfaceFromHit(hit);

                if (surface != null)
                    totalOcclusion +=
                        GetOcclusionStrength(surface.OcclusionPower);
            }
        }

        return totalOcclusion / softOcclusionRays;
    }
}