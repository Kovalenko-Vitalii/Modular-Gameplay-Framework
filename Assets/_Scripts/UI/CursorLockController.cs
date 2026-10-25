using System.Collections.Generic;
using UnityEngine;

public sealed class CursorLockController {
    public bool IsLocked { get; private set; } = false;

    /*
    readonly HashSet<string> _requests = new();

    public void ChangeState(string name, bool isLocked) { // !!! reason could be changed to object but string works for now !!!
        bool changed;

        if (isLocked)
            changed = _requests.Add(name);
        else
            changed = _requests.Remove(name);

        if (!changed)
            return;
        
        Recalculate();
    }

    void Recalculate() {
        bool shouldLock = _requests.Count > 0;
        if (shouldLock == IsLocked)
            return;

        SetLocked(shouldLock);
    }
    */

    public void LockCursor() {
        IsLocked = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor() {
        IsLocked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SetLocked(bool locked) {
        if (locked)
            LockCursor();
        else
            UnlockCursor();
    }
}