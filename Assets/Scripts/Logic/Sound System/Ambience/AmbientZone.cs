using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AmbientZone : MonoBehaviour
{
    [SerializeField] private AmbientProfile profile;

    [Tooltip("Distance from zone centre at which influence reaches zero.")]
    [SerializeField] private float blendDistance = 10f;

    public AmbientProfile Profile => profile;
    public float BlendDistance => blendDistance;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        AmbientManager.Instance.RegisterZone(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        AmbientManager.Instance.UnregisterZone(this);
    }

    /// <summary>Returns 0-1 based on listener distance from zone centre.</summary>
    public float GetInfluence(Vector3 listenerPosition)
    {
        float distance = Vector3.Distance(transform.position, listenerPosition);
        return 1f - Mathf.Clamp01(distance / blendDistance);
    }
}