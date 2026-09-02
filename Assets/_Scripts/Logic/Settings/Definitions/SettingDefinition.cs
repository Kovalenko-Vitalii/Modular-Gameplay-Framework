using System;
using System.Collections.Generic;

public class SettingDefinition :ISettingRow
{
    // Interface implementation
    public string Name { get; }
    public string CurrentValue => Options[CurrentIndex];
    public bool SecondaryEnabled => Options.Count > 1;
    public SettingRowMode Mode => SettingRowMode.Cycle;
    public event Action Changed; // Callback with no payload, for ui

    // Class members
    public IReadOnlyList<string> Options { get; }
    public int CurrentIndex { get; private set; }
    private readonly Action<int> onChanged; // Callback with payload

    // Constructor
    public SettingDefinition(
        string name,
        IReadOnlyList<string> options,
        int currentIndex,
        Action<int> onChanged)
    {
        Name = name;
        Options = options;
        CurrentIndex = currentIndex;
        this.onChanged = onChanged;
    }

    // Interface implementation
    public void PrimaryAction() => Next();
    public void SecondaryAction() => Previous();

    // Methods to change the current index
    public void Next()
    {
        CurrentIndex++;
        if (CurrentIndex >= Options.Count) CurrentIndex = 0;
        onChanged?.Invoke(CurrentIndex);
        Changed?.Invoke();
    }
    public void Previous()
    {
        CurrentIndex--;
        if (CurrentIndex < 0) CurrentIndex = Options.Count - 1;
        onChanged?.Invoke(CurrentIndex);
        Changed?.Invoke();
    }
}