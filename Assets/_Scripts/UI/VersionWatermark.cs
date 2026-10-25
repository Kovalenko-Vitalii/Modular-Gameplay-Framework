using TMPro;
using UnityEngine;

public class VersionWatermark : MonoBehaviour {
    [SerializeField] TextMeshProUGUI text;
    private void Start() => text.text = $"'{Application.productName}' ver '{Application.version}'";
}
