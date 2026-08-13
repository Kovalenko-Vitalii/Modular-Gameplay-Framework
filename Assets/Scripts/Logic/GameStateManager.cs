using System;
using UnityEngine;

// <summary>
// Singleton class that manages the game state
// </summary>
[DefaultExecutionOrder(-2000)]
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    string TAG = "GameStateManager";
    public GameState Current { get; private set; } = GameState.MainMenu;

    public static event Action<GameState> StateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        UIWindowManager.WindowChanged += OnWindowChanged;
    }

    private void OnDisable()
    {
        UIWindowManager.WindowChanged -= OnWindowChanged;
    }

    private void OnWindowChanged(UIWindowId window)
    {
        // Don't let a window opening/closing clobber Loading/Cutscene/Menu.
        if (Current != GameState.Gameplay && Current != GameState.Paused)
            return;

        SetState(window == UIWindowId.None ? GameState.Gameplay : GameState.Paused);
    }

    public void SetState(GameState newState)
    {
        if (newState == Current)
            return;

        Current = newState;
        ApplyStateConsequences(newState);
        StateChanged?.Invoke(newState);
    }

    private void ApplyStateConsequences(GameState state)
    {
        bool paused = state == GameState.Paused;

        GameLog.Log(TAG, "State changed to " + state);
    }
}

public enum GameState
{
    Gameplay,
    Paused,
    Loading,
    Cutscene,
    MainMenu
}