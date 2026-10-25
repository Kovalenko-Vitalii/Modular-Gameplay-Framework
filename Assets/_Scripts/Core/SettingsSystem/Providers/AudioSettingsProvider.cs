using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class AudioSettingsProvider : ISettingsCategoryProvider {
    public string CategoryName => "Audio";

    private WwiseAudioSettings _audioSettings;

    [Inject]
    void Construct(WwiseAudioSettings wwiseAudioSettings) {
        _audioSettings = wwiseAudioSettings;
    }

    public List<ISettingRow> BuildSettings()
    {
        return new List<ISettingRow>
        {
             CreateVolumeSetting(
                "Master Volume",
                () => _audioSettings.MasterVolume,
                _audioSettings.SetMasterVolume),

            CreateVolumeSetting(
                "SFX Volume",
                () => _audioSettings.SFXVolume,
                _audioSettings.SetSFXVolume),

            CreateVolumeSetting(
                "Music Volume",
                () => _audioSettings.MusicVolume,
                _audioSettings.SetMusicVolume),

            CreateVolumeSetting(
                "Voice Volume",
                () => _audioSettings.VoiceVolume,
                _audioSettings.SetVoiceVolume),

            CreateVolumeSetting(
                "UI Volume",
                () => _audioSettings.UIVolume,
                _audioSettings.SetUIVolume)
        };
    }

    private SettingDefinition CreateVolumeSetting( string label, Func<float> getCurrent, Action<float> setValue) {
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