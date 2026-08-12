using System.Collections.Generic;

public class GameSettingsProvider : ISettingsCategoryProvider
{
    public string CategoryName => "Game";

    public List<ISettingRow> BuildSettings()
    {
        return new List<ISettingRow>
        {
            CreateFpsCounterSetting()
        };
    }

    private SettingDefinition CreateFpsCounterSetting()
    {
        var options = new List<string> { "Disabled", "Enabled" };
        int startIndex = FPSCounter.Instance != null && FPSCounter.Instance.IsEnabled ? 1 : 0;

        return new SettingDefinition(
            "FPS Counter",
            options,
            startIndex,
            index => FPSCounter.Instance?.SetEnabled(index == 1));
    }
}