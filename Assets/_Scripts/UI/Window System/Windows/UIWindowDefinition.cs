using UnityEngine;

[CreateAssetMenu(menuName = "UI/Window Definition", fileName = "Window_")]
public class UIWindowDefinition : ScriptableObject
{
    [SerializeField] string id;

    [Header("Behavior")]
    [SerializeField] bool pausesSimulation;
    [SerializeField] bool locksPlayerInput;
    [SerializeField] bool closableWithEsc = true;

    public string Id =>
        string.IsNullOrEmpty(id) ? name : id;

    public bool PausesSimulation => pausesSimulation;
    public bool LocksPlayerInput => locksPlayerInput;
    public bool ClosableWithEsc => closableWithEsc;
}