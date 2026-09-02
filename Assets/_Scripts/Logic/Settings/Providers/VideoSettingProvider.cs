using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VideoSettingsProvider : ISettingsCategoryProvider
{
    public string CategoryName => "Video";

    private static readonly (int w, int h)[] ResolutionValues =
    {
        (1920, 1080), (2560, 1440), (3840, 2160)
    };

    public List<ISettingRow> BuildSettings()
    {
        return new List<ISettingRow>
        {
            CreateResolutionSetting(),
            CreateFullscreenSetting(),
            CreateVSyncSetting(),
            CreateQualitySetting()
        };
    }

    private SettingDefinition CreateResolutionSetting()
    {
        var labels = ResolutionValues.Select(r => $"{r.w} x {r.h}").ToList();

        int startIndex = 0;
        for (int i = 0; i < ResolutionValues.Length; i++)
        {
            if (ResolutionValues[i].w == Screen.width && ResolutionValues[i].h == Screen.height)
            {
                startIndex = i;
                break;
            }
        }

        return new SettingDefinition("Resolution", labels, startIndex,
            index =>
            {
                var (w, h) = ResolutionValues[index];
                Screen.SetResolution(w, h, Screen.fullScreen);
            });
    }

    private SettingDefinition CreateFullscreenSetting()
    {
        var options = new List<string> { "Enabled", "Disabled" };
        int startIndex = Screen.fullScreen ? 0 : 1;

        return new SettingDefinition("Fullscreen", options, startIndex,
            index => Screen.fullScreen = index == 0);
    }

    private SettingDefinition CreateVSyncSetting()
    {
        var options = new List<string> { "Enabled", "Disabled" };
        int startIndex = QualitySettings.vSyncCount > 0 ? 0 : 1;

        return new SettingDefinition("VSync", options, startIndex,
            index => QualitySettings.vSyncCount = index == 0 ? 1 : 0);
    }

    private SettingDefinition CreateQualitySetting()
    {
        var options = new List<string> { "Low", "Medium", "High", "Ultra" };
        return new SettingDefinition("Quality", options, QualitySettings.GetQualityLevel(),
            index => QualitySettings.SetQualityLevel(index));
    }
}