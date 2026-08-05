using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class AudioOcclusionSystem : MonoBehaviour
{
    public static AudioOcclusionSystem Instance { get; private set; }

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

    private void Awake()
    {
        Instance = this;

        var audioListener = FindFirstObjectByType<AudioListener>();
        if (audioListener != null)
            listener = audioListener.transform;
        else
            Debug.LogWarning("[AudioOcclusionSystem] No AudioListener found in scene.");
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Register(OccludableAudio source)
    {
        if (!sources.Contains(source))
            sources.Add(source);
    }

    public void Unregister(OccludableAudio source)
    {
        int index = sources.IndexOf(source);
        if (index < 0) return;

        sources.RemoveAt(index);
        if (cursor > index) cursor--;
    }

    private void Update()
    {
        if (listener == null || sources.Count == 0)
            return;

        timer += Time.deltaTime;
        if (timer < updateInterval)
            return;
        timer = 0f;

        int checksThisTick = Mathf.Min(maxChecksPerTick, sources.Count);
        for (int i = 0; i < checksThisTick; i++)
        {
            cursor %= sources.Count;
            UpdateOcclusion(sources[cursor]);
            cursor++;
        }
    }

    private void UpdateOcclusion(OccludableAudio source)
    {
        Vector3 sourcePos = source.transform.position;
        Vector3 toListener = listener.position - sourcePos;
        float distance = toListener.magnitude;

        if (distance > maxOcclusionCheckDistance)
        {
            source.Occlusion = 0f;
            return;
        }

        Vector3 dir = toListener / Mathf.Max(distance, 0.0001f);

        if (!useSoftOcclusion)
        {
            source.Occlusion = Physics.Raycast(
                sourcePos, dir, distance, obstacleMask, QueryTriggerInteraction.Ignore)
                ? 1f
                : 0f;
            return;
        }

        source.Occlusion = SampleSoftOcclusion(sourcePos, dir, distance);
    }

    private float SampleSoftOcclusion(Vector3 sourcePos, Vector3 dir, float distance)
    {
        Vector3 right = Vector3.Cross(dir, Vector3.up);
        if (right.sqrMagnitude < 0.0001f) right = Vector3.Cross(dir, Vector3.forward);
        right = right.normalized * softOcclusionSampleRadius;
        Vector3 up = Vector3.Cross(right.normalized, dir).normalized * softOcclusionSampleRadius;

        int blocked = 0;
        for (int i = 0; i < softOcclusionRays; i++)
        {
            Vector3 offset = i switch
            {
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

            if (Physics.Raycast(origin, delta / Mathf.Max(dist, 0.0001f), dist, obstacleMask, QueryTriggerInteraction.Ignore))
                blocked++;
        }

        return (float)blocked / softOcclusionRays;
    }
}