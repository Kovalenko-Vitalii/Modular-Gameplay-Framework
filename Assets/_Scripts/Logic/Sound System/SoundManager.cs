using UnityEngine;
using UnityEngine.Audio;
using VContainer;

// <summary>
// Singleton class that manages all audio in the game, including UI, subtitles, and world sounds
// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] AudioMixerGroup worldMixerGroup;

    private const string MASTER_KEY = "audio_master";
    private const string UI_KEY = "audio_ui";
    private const string SUBTITLE_KEY = "audio_subtitle";
    private const string WORLD_KEY = "audio_world";

    private const string MIXER_MASTER = "MasterVolume";
    private const string MIXER_UI = "UIVolume";
    private const string MIXER_SUBTITLE = "SubtitleVolume";
    private const string MIXER_WORLD = "WorldVolume";

    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("2D Audio Sources")]
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource subtitleSource;

    [Header("Volumes")]
    [SerializeField, Range(0f, 1f)] private float volumeMaster = 1f;
    [SerializeField, Range(0f, 1f)] private float volumeUI = 1f;
    [SerializeField, Range(0f, 1f)] private float volumeSubtitle = 1f;
    [SerializeField, Range(0f, 1f)] private float volumeWorld = 1f;

    public float MasterVolume => volumeMaster;
    public float UIVolume => volumeUI;
    public float SubtitleVolume => volumeSubtitle;
    public float WorldVolume => volumeWorld;

    GameStateManager _gameStateManager;

    [Inject]
    void Construct(GameStateManager gameStateManager) {
        _gameStateManager = gameStateManager;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (uiSource != null)
            uiSource.ignoreListenerPause = true;

        LoadVolumes();
    }


    private void OnEnable()
    {
        _gameStateManager.PauseChanged += OnPausedChanged;
        SetAudioPaused(_gameStateManager.IsPaused); 
    }

    private void OnDisable() => _gameStateManager.PauseChanged -= OnPausedChanged;
    private void OnPausedChanged(bool isPaused) =>SetAudioPaused(isPaused);

    private void Start() => ApplyVolumes();
    public void SetAudioPaused(bool paused) => AudioListener.pause = paused;
   
    public void PlayUI(AudioClip clip, float volumeMul = 1f)
    {
        if (uiSource == null || clip == null)
            return;

        uiSource.PlayOneShot(clip, volumeMul);
    }

    public void PlaySubtitleSound(AudioClip clip, float volumeMul = 1f)
    {
        if (subtitleSource == null || clip == null)
            return;

        subtitleSource.PlayOneShot(clip, volumeMul);
    }

    // Plays a 3D sound at the specified position in the world
    public void PlayWorldOneShot(AudioClip clip, Vector3 position, float volumeMul = 1f, float pitch = 1f, float range = 10f)
    {
        if (clip == null) return;

        GameObject go = new GameObject("OneShotAudio");
        go.transform.position = position;

        AudioSource source = go.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = worldMixerGroup; 
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.maxDistance = range;
        source.pitch = pitch;
        source.PlayOneShot(clip, volumeMul);

        Destroy(go, clip.length / Mathf.Max(0.01f, Mathf.Abs(pitch)) + 0.1f);
    }

    public void SetMasterVolume(float value)
    {
        volumeMaster = Mathf.Clamp01(value);
        ApplyVolumes();
        SaveVolumes();
    }

    public void SetUIVolume(float value)
    {
        volumeUI = Mathf.Clamp01(value);
        ApplyVolumes();
        SaveVolumes();
    }

    public void SetSubtitleVolume(float value)
    {
        volumeSubtitle = Mathf.Clamp01(value);
        ApplyVolumes();
        SaveVolumes();
    }

    public void SetWorldVolume(float value)
    {
        volumeWorld = Mathf.Clamp01(value);
        ApplyVolumes();
        SaveVolumes();
    }

    private void ApplyVolumes()
    {
        if (audioMixer == null)
            return;

        SetMixerVolume(MIXER_MASTER, volumeMaster);
        SetMixerVolume(MIXER_UI, volumeUI);
        SetMixerVolume(MIXER_SUBTITLE, volumeSubtitle);
        SetMixerVolume(MIXER_WORLD, volumeWorld);
    }

    private void SetMixerVolume(string parameterName, float linearValue)
    {
        float dbValue = LinearToDb(linearValue);

        bool success = audioMixer.SetFloat(parameterName, dbValue);

#if UNITY_EDITOR
        if (!success)
            Debug.LogWarning($"AudioMixer parameter not found: {parameterName}", this);
#endif
    }

    private float LinearToDb(float value)
    {
        if (value <= 0.0001f)
            return -80f;

        return Mathf.Log10(value) * 20f;
    }

    private void LoadVolumes()
    {
        volumeMaster = PlayerPrefs.GetFloat(MASTER_KEY, 1f);
        volumeUI = PlayerPrefs.GetFloat(UI_KEY, 1f);
        volumeSubtitle = PlayerPrefs.GetFloat(SUBTITLE_KEY, 1f);
        volumeWorld = PlayerPrefs.GetFloat(WORLD_KEY, 1f);
    }

    private void SaveVolumes()
    {
        PlayerPrefs.SetFloat(MASTER_KEY, volumeMaster);
        PlayerPrefs.SetFloat(UI_KEY, volumeUI);
        PlayerPrefs.SetFloat(SUBTITLE_KEY, volumeSubtitle);
        PlayerPrefs.SetFloat(WORLD_KEY, volumeWorld);
        PlayerPrefs.Save();
    }
}