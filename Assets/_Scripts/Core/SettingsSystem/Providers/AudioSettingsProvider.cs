using System.Collections.Generic;
using UnityEngine;

// <summary>
// Provides audio settings for the settings menu, including master volume, UI volume, subtitle volume, and world volume.
// </summary>
public class AudioSettingsProvider : ISettingsCategoryProvider {
    public string CategoryName => "Audio";

    SoundManager _soundManager;

    public AudioSettingsProvider(SoundManager soundManager) { 
        _soundManager = soundManager;
    }

    public List<ISettingRow> BuildSettings()
    {
        return new List<ISettingRow>
        {
            CreateVolumeSetting("Master Volume",
                () => _soundManager.MasterVolume,
                _soundManager.SetMasterVolume),

            CreateVolumeSetting("UI Volume",
                () => _soundManager.UIVolume,
                _soundManager.SetUIVolume),

            CreateVolumeSetting("Subtitle Volume",
                () => _soundManager.SubtitleVolume,
                _soundManager.SetSubtitleVolume),

            CreateVolumeSetting("World Volume",
                () =>_soundManager.WorldVolume,
                _soundManager.SetWorldVolume)
        };
    }

    // Function to create a volume setting with a label, getter, and setter for the current value
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

    // Converts an index (0-10) to a linear volume value (0.0-1.0)
    private float IndexToLinear(int index) => index / 10f;

    // Converts a linear volume value (0.0-1.0) to an index (0-10)
    private int LinearToIndex(float linear) => Mathf.RoundToInt(linear * 10f);
}