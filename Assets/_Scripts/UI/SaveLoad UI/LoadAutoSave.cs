using SaveSystem;
using UnityEngine;
using UnityEngine.UI;

public class LoadAutoSave : MonoBehaviour
{
    [SerializeField] Button button;

    void Start()
    {
        var acvtiveProfile = SaveService.Instance.ActiveProfile;
        button.onClick.AddListener(() => GameFlowController.Instance.StartManual(acvtiveProfile.id, acvtiveProfile.autoSave.id));
    }
}
