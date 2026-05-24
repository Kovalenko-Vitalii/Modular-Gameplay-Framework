using UnityEngine;

public sealed class CursorLockController : MonoBehaviour
{
    [SerializeField] private bool lockOnStart = true;
    [SerializeField] private bool hideCursorWhenLocked = true;

    public bool IsLocked { get; private set; }

    private void Start()
    {
        if (lockOnStart)
            LockCursor();
        else
            UnlockCursor();
    }

    public void LockCursor()
    {
        IsLocked = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = !hideCursorWhenLocked;
    }

    public void UnlockCursor()
    {
        IsLocked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SetLocked(bool locked)
    {
        if (locked)
            LockCursor();
        else
            UnlockCursor();
    }
}