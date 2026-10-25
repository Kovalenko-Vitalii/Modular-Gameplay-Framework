using UnityEngine;

public sealed class WwiseAudioSettings {
    private const string MASTER_RTPC = "GP_Master_Volume";
    private const string SFX_RTPC = "GP_SFX_Volume";
    private const string MUSIC_RTPC = "GP_Music_Volume";
    private const string VOICE_RTPC = "GP_Voice_Volume";
    private const string UI_RTPC = "GP_UI_Volume";

    public float MasterVolume { get; private set; } = 1f;
    public float SFXVolume { get; private set; } = 1f;
    public float MusicVolume { get; private set; } = 1f;
    public float VoiceVolume { get; private set; } = 1f;
    public float UIVolume { get; private set; } = 1f;

    public void SetMasterVolume(float value) {
        MasterVolume = Mathf.Clamp01(value);
        AkUnitySoundEngine.SetRTPCValue(MASTER_RTPC, MasterVolume);
    }

    public void SetSFXVolume(float value) {
        SFXVolume = Mathf.Clamp01(value);
        AkUnitySoundEngine.SetRTPCValue(SFX_RTPC, SFXVolume);
    }

    public void SetMusicVolume(float value) {
        MusicVolume = Mathf.Clamp01(value);
        AkUnitySoundEngine.SetRTPCValue(MUSIC_RTPC, MusicVolume);
    }

    public void SetVoiceVolume(float value) {
        VoiceVolume = Mathf.Clamp01(value);
        AkUnitySoundEngine.SetRTPCValue(VOICE_RTPC, VoiceVolume);
    }

    public void SetUIVolume(float value) {
        UIVolume = Mathf.Clamp01(value);
        AkUnitySoundEngine.SetRTPCValue(UI_RTPC, UIVolume);
    }
}