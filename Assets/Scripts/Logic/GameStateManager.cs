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
    public GameState Current { get; private set; } = GameState.Gameplay;

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
        InputListener.ActionPressed += OnActionPressed;
    }

    private void OnDisable()
    {
        InputListener.ActionPressed -= OnActionPressed;
    }

    private void OnActionPressed(GameAction action)
    {
        if (action == GameAction.Esc || action == GameAction.Inventory)
            TogglePause();
    }

    public void TogglePause()
    {
        SetState(Current == GameState.Gameplay ? GameState.Paused : GameState.Gameplay);
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
    Menu
}