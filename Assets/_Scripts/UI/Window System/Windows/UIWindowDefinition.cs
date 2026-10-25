using UnityEngine;

[CreateAssetMenu(menuName = "UI/Window Definition", fileName = "Window_")]
public class UIWindowDefinition : ScriptableObject {
    [Header("Behavior")]
    [SerializeField] bool closableWithEsc = true; 

    public bool ClosableWithEsc => closableWithEsc;
}