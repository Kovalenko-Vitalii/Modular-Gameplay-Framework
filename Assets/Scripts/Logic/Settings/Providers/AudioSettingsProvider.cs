using System.Collections.Generic;
using UnityEngine;

public class AudioSettingsProvider : ISettingsCategoryProvider
{
    public string CategoryName => "Audio";

    public List<ISettingRow> BuildSettings()
    {
        return new List<ISettingRow>
        {
            CreateVolumeSetting("Master Volume",
                () => SoundManager.Instance.MasterVolume,
                SoundManager.Instance.SetMasterVolume),

            CreateVolumeSetting("UI Volume",
                () => SoundManager.Instance.UIVolume,
                SoundManager.Instance.SetUIVolume),

            CreateVolumeSetting("Subtitle Volume",
                () => SoundManager.Instance.SubtitleVolume,
                SoundManager.Instance.SetSubtitleVolume),

            CreateVolumeSetting("World Volume",
                () => SoundManager.Instance.WorldVolume,
                SoundManager.Instance.SetWorldVolume)
        };
    }

    private SettingDefinition CreateVolumeSetting(
        string label,
        System.Func<float> getCurrent,
        System.Action<float> setValue)
    {
        var options = new List<string>();
        for (int i = 0; i <= 10; i++)
            options.Add(i.ToString());

        int startIndex = LinearToIndex(getCurrent());

        return new SettingDefinition(
            label,
            options,
            startIndex,
            index => setValue(IndexToLinear(index)));
    }

    private float IndexToLinear(int index) => index / 10f;

    private int LinearToIndex(float linear) => Mathf.RoundToInt(linear * 10f);
}