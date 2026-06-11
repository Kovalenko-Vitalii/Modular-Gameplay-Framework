using UnityEngine;

public class AmbienceAudio : MonoBehaviour
{
    public GameObject target;
    public Collider area;
    void Update()
    {
        transform.position = area.ClosestPoint(target.transform.position);
    }
}
