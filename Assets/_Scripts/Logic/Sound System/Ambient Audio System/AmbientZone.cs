using UnityEngine;
using VContainer;
[RequireComponent(typeof(Collider))]
public class AmbientZone : MonoBehaviour
{
    [SerializeField] AmbientProfile profile;
    [SerializeField] int priority;
    [SerializeField] float blendDistance = 5f;
   
    public AmbientProfile Profile => profile;
    public int Priority => priority;

    AmbientManager _ambientManager;

    [Inject]
    public void Construct(AmbientManager ambientManager) {
        _ambientManager = ambientManager;
    }
    private void OnDestroy() {
        _ambientManager.UnregisterZone(this);
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player"))
            return;

        _ambientManager.RegisterZone(this);
    }



    private void OnTriggerExit(Collider other) {
        if (!other.CompareTag("Player"))
            return;

        _ambientManager.UnregisterZone(this);
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