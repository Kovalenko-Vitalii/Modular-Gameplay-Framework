using System;
using UnityEngine.InputSystem;

public class RebindSettingDefinition : ISettingRow
{
    // Interface implementation
    public string Name { get; }
    public string CurrentValue =>
            isWaiting
                ? "Press any key..."
                : action.GetBindingDisplayString(bindingIndex, InputBinding.DisplayStringOptions.DontIncludeInteractions); 
    public bool SecondaryEnabled => action.bindings[bindingIndex].hasOverrides && !isWaiting;
    public SettingRowMode Mode => SettingRowMode.Rebind;
    private readonly UnityEngine.InputSystem.InputAction action;
    public event Action Changed;

    // Class members
    private readonly int bindingIndex;
    private readonly Action onSaved; // same as onChanged, but for rebinding, since we don't have a payload to pass
    private InputActionRebindingExtensions.RebindingOperation activeOp;
    private bool isWaiting;

    // Constructor
    public RebindSettingDefinition(string name, UnityEngine.InputSystem.InputAction action, int bindingIndex, Action onSaved = null)
    {
        Name = name;
        this.action = action;
        this.bindingIndex = bindingIndex;
        this.onSaved = onSaved;
    }

    // Interface implementation
    // PrimaryAction starts the rebinding process for the specified action and binding index
    public void PrimaryAction()
    {
        if (isWaiting) return;

        isWaiting = true;
        Changed?.Invoke();

        bool wasEnabled = action.enabled;
        action.Disable();

        activeOp = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(op =>
            {
                op.Dispose();
                if (wasEnabled) action.Enable();
                isWaiting = false;
                onSaved?.Invoke();
                Changed?.Invoke();
            })
            .OnCancel(op =>
            {
                op.Dispose();
                if (wasEnabled) action.Enable();
                isWaiting = false;
                Changed?.Invoke();
            });

        activeOp.Start();
    }

    // SecondaryAction removes any binding overrides for the specified action and binding index
    public void SecondaryAction()
    {
        if (isWaiting) return;

        action.RemoveBindingOverride(bindingIndex);
        onSaved?.Invoke();
        Changed?.Invoke();
    }

    // CancelIfActive cancels the active rebinding operation if it is currently in progress
    public void CancelIfActive()
    {
        activeOp?.Cancel();
        activeOp?.Dispose();
        activeOp = null;
    }
}