using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class AudioSpline : MonoBehaviour
{
    [SerializeField] private SplineContainer spline;
    Transform listener;

    void Start() => listener = GameObject.FindGameObjectWithTag("Player").transform;
    
    private void FixedUpdate()
    {
        if (spline == null || listener == null)
            return;

        float3 localPlayerPosition = spline.transform.InverseTransformPoint(listener.position);

        SplineUtility.GetNearestPoint(
            spline.Spline,
            localPlayerPosition,
            out float3 nearest,
            out float t);

        transform.position = spline.transform.TransformPoint(nearest);
    }
}