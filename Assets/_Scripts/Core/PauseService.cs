using System;
using System.Collections.Generic;

public class PauseService {
    private readonly HashSet<string> pauseReasons = new();
    public bool IsPaused { get; private set; } = false;

    public event Action<bool> PauseChanged;

    /// <summary>
    /// Sets a pause reason. If active is true, the reason is added to the list of reasons to pause.
    /// If active is false, the reason is removed from the list of reasons to pause.
    /// </summary>
    public void SetPauseReason(string reason, bool active) { // !!! reason could be changed to object but string works for now !!!
        bool changed;

        if (active)
            changed = pauseReasons.Add(reason);
        else
            changed = pauseReasons.Remove(reason);

        if (!changed)
            return;

        RecomputePause();
    }

    /// <summary> If any pause reasons are active, pause the game </summary>
    private void RecomputePause() {
        bool shouldPause = pauseReasons.Count > 0;
        if (shouldPause == IsPaused)
            return;

        IsPaused = shouldPause;
        PauseChanged?.Invoke(IsPaused);
    }
}
