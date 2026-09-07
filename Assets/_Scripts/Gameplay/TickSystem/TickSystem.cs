using System.Collections.Generic;
using UnityEngine;
using VContainer;

// <summary>
// Ticking system that allows manage simulation process
// </summary>
[DefaultExecutionOrder(-1000)]
public class TickSystem : MonoBehaviour {
    const string TAG = "PlayerTickSystem";

    // Lists of registered tickable objects
    readonly List<ITick> ticks = new();
    readonly List<ILateTick> lateTicks = new();

    // Buffers to avoid modifying the tick lists while iterating over them
    readonly List<ITick> tickBuffer = new();
    readonly List<ILateTick> lateTickBuffer = new();

    bool isTicking = true;

    GameStateManager _gameStateManager;

    [Inject]
    void Construct(GameStateManager gameStateManager) {
        _gameStateManager = gameStateManager;
    }

    // acceptable level of glue 
    private void OnEnable()
    {
        _gameStateManager.PauseChanged += OnPausedChanged;
        isTicking = !_gameStateManager.IsPaused;
    }

    private void OnDisable() => _gameStateManager.PauseChanged -= OnPausedChanged; 

    private void OnPausedChanged(bool isPaused) =>isTicking = !isPaused;

    // <summary>
    // Registers an object that implements IPlayerTick or IPlayerLateTick to be ticked every frame
    // </summary>
    public void Register(object tickable)
    {
        bool registered = false;

        if (tickable is ITick tick && !ticks.Contains(tick))
        {
            ticks.Add(tick);
            registered = true;
        }

        if (tickable is ILateTick lateTick && !lateTicks.Contains(lateTick))
        {
            lateTicks.Add(lateTick);
            registered = true;
        }

        if (!registered)  
            GameLog.Warning(TAG, $"Tried to register non-tickable object: {tickable}");
    }

    // <summary>
    // Unregisters an object that implements IPlayerTick or IPlayerLateTick from being ticked every frame
    // </summary>
    public void Unregister(object tickable)
    {
        if (tickable is ITick tick)
            ticks.Remove(tick);

        if (tickable is ILateTick lateTick)
            lateTicks.Remove(lateTick);
    }

    // <summary>
    // Ticks all registered IPlayerTick objects every frame
    // Analog to Update()
    // </summary>
    void Update()
    {
        if (!isTicking) return;

        float dt = Time.deltaTime;

        tickBuffer.Clear();
        tickBuffer.AddRange(ticks);

        for (int i = 0; i < tickBuffer.Count; i++)
            tickBuffer[i].Tick(dt);
    }

    // <summary>
    // Ticks all registered IPlayerLateTick objects every frame
    // Analog to LateUpdate()
    // </summary>
    void LateUpdate()
    {
        if (!isTicking) return;

        float dt = Time.deltaTime;

        lateTickBuffer.Clear();
        lateTickBuffer.AddRange(lateTicks);

        for (int i = 0; i < lateTickBuffer.Count; i++)
            lateTickBuffer[i].LateTick(dt);
    }
}