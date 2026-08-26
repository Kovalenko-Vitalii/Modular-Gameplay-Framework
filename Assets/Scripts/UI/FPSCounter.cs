using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public static FPSCounter Instance { get; private set; }

    [SerializeField] private TMP_Text fpsText;
    [SerializeField] private float updateInterval = 0.5f;

    private const string PREF_KEY = "fps_counter_enabled";

    private float timer;
    private int frames;

    public bool IsEnabled { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        bool saved = PlayerPrefs.GetInt(PREF_KEY, 0) == 1;
        SetEnabled(saved);
    }

    public void SetEnabled(bool enabled)
    {
        IsEnabled = enabled;

        if (fpsText != null)
            fpsText.gameObject.SetActive(enabled);

        PlayerPrefs.SetInt(PREF_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void Update()
    {
        if (!IsEnabled || fpsText == null)
            return;

        frames++;
        timer += Time.unscaledDeltaTime;

        if (timer >= updateInterval)
        {
            float fps = frames / timer;
            fpsText.text = $"{fps:F0} FPS";
            frames = 0;
            timer = 0f;
        }
    }
}