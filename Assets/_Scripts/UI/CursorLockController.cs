using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public sealed class CursorLockController : IStartable, IDisposable {
    public bool IsLocked { get; private set; }

    PauseService _pauseService;

    [Inject]
    void Construct(PauseService pauseService) => _pauseService = pauseService;
       
    public void Start() => _pauseService.PauseChanged += OnPausedChanged;
    public void Dispose() => _pauseService.PauseChanged -= OnPausedChanged;
        
    private void OnPausedChanged(bool isPaused) => SetLocked(!isPaused); 

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