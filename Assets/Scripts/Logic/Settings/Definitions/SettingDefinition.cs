using System;
using System.Collections.Generic;

public class SettingDefinition :ISettingRow
{
    public string Name { get; }
    public IReadOnlyList<string> Options { get; }

    public int CurrentIndex { get; private set; }

    public SettingRowMode Mode => SettingRowMode.Cycle;

    public event Action Changed;
    private readonly Action<int> onChanged;

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

    public string CurrentValue => Options[CurrentIndex];
    public bool SecondaryEnabled => Options.Count > 1;

    public void PrimaryAction() => Next();
    public void SecondaryAction() => Previous();

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