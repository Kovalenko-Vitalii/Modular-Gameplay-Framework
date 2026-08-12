using System;

public enum SettingRowMode
{
    Cycle,
    Rebind
}

public interface ISettingRow
{
    string Name { get; }
    string CurrentValue { get; }
    bool SecondaryEnabled { get; }
    SettingRowMode Mode { get; }

    event Action Changed;

    void PrimaryAction();
    void SecondaryAction();
}