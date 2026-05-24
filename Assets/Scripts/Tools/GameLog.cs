using UnityEngine;

public static class GameLog
{
    public static bool Enabled = true;

    public static void Log(string tag, string msg)
    {
        if (!Enabled) return;
        Debug.Log($"[{tag}] f={Time.frameCount} t={Time.unscaledTime:F2} {msg}");
    }

    public static void Warning(string tag, string msg)
    {
        if (!Enabled) return;
        Debug.LogWarning($"[{tag}] f={Time.frameCount} t={Time.unscaledTime:F2} {msg}");
    }

    public static void Error(string tag, string msg)
    {
        if (!Enabled) return;
        Debug.LogError($"[{tag}] f={Time.frameCount} t={Time.unscaledTime:F2} {msg}");
    }
}