using UnityEngine;

[CreateAssetMenu(menuName = "UI/Window Definition", fileName = "Window_")]
public class UIWindowDefinition : ScriptableObject
{
    [SerializeField] string id;

    [Header("Behavior")]
    [SerializeField] bool closableWithEsc = true; 

    public string Id => string.IsNullOrEmpty(id) ? name : id;

    public bool ClosableWithEsc => closableWithEsc;
}