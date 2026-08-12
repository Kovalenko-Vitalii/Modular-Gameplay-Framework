using System;
using UnityEngine.InputSystem;

public class RebindSettingDefinition : ISettingRow
{
    public string Name { get; }

    private readonly InputAction action;
    private readonly int bindingIndex;
    private readonly Action onSaved;

    private InputActionRebindingExtensions.RebindingOperation activeOp;
    private bool isWaiting;

    public event Action Changed;

    public SettingRowMode Mode => SettingRowMode.Rebind;

    public RebindSettingDefinition(string name, InputAction action, int bindingIndex, Action onSaved = null)
    {
        Name = name;
        this.action = action;
        this.bindingIndex = bindingIndex;
        this.onSaved = onSaved;
    }

    public string CurrentValue =>
        isWaiting
            ? "Press any key..."
            : action.GetBindingDisplayString(bindingIndex, InputBinding.DisplayStringOptions.DontIncludeInteractions);

    public bool SecondaryEnabled => action.bindings[bindingIndex].hasOverrides && !isWaiting;

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

    public void SecondaryAction()
    {
        if (isWaiting) return;

        action.RemoveBindingOverride(bindingIndex);
        onSaved?.Invoke();
        Changed?.Invoke();
    }

    public void CancelIfActive()
    {
        activeOp?.Cancel();
        activeOp?.Dispose();
        activeOp = null;
    }
}