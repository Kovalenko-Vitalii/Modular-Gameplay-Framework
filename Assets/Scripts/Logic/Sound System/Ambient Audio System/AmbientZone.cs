using UnityEngine;
[RequireComponent(typeof(Collider))]
public class AmbientZone : MonoBehaviour
{
    [SerializeField] AmbientProfile profile;
    [SerializeField] int priority;
    [SerializeField] float blendDistance = 5f;
   
    public AmbientProfile Profile => profile;
    public int Priority => priority;

    private void OnDestroy() {
        AmbientManager.Instance.UnregisterZone(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        AmbientManager.Instance.RegisterZone(this);
    }



    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        AmbientManager.Instance.UnregisterZone(this);
    }



    public float GetInfluence(Vector3 position)
    {
        if (blendDistance <= 0)
            return 1;


        float distance =
            Vector3.Distance(
                transform.position,
                position);

        return 1f - Mathf.Clamp01(distance / blendDistance);
    }
}