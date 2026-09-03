using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public sealed class CursorLockController : IStartable, IDisposable {
    public bool IsLocked { get; private set; }

    GameStateManager _gameStateManager;

    [Inject]
    void Construct(GameStateManager gameStateManager) {
        _gameStateManager = gameStateManager;
    }

    public void Start() {
        _gameStateManager.PauseChanged += OnPausedChanged;
        SetLocked(!_gameStateManager.IsPaused);
        LockCursor();
    }

    public void Dispose() {
        _gameStateManager.PauseChanged -= OnPausedChanged;
    }

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