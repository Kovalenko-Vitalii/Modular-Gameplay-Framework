using System;
using System.Collections.Generic;

public class PauseService {
    public bool IsPaused { get; private set; } = false;
    readonly HashSet<string> _requests = new();

    public event Action<bool> PauseChanged;

    public void RequestPause(string name, bool paused) { // !!! reason could be changed to object but string works for now !!!
        bool changed;

        if (paused)
            changed = _requests.Add(name);
        else
            changed = _requests.Remove(name);

        if (!changed)
            return;

        Recalculate();
    }

    /// <summary> If any pause reasons are active, pause the game </summary>
    void Recalculate() {
        bool shouldPause = _requests.Count > 0;
        if (shouldPause == IsPaused)
            return;

        IsPaused = shouldPause;
        PauseChanged?.Invoke(IsPaused);
    }
}
