using UnityEngine;

[CreateAssetMenu(menuName = "UI/Window Definition", fileName = "Window_")]
public class UIWindowDefinition : ScriptableObject
{
    [SerializeField] private string id;

    [Header("Behavior")]
    public bool pausesGame = false;
    public bool closableWithEsc = true;

    public string Id => string.IsNullOrEmpty(id) ? name : id;

    public override string ToString() => Id;
}