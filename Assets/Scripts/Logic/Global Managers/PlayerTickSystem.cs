using System.Collections.Generic;
using UnityEngine;

// <summary>
// Singleton system that manages player tickable objects and calls their Tick() and LateTick() methods every frame
// </summary>
[DefaultExecutionOrder(-1000)]
public class PlayerTickSystem : MonoBehaviour
{
    const string TAG = "PlayerTickSystem";

    public static PlayerTickSystem Instance { get; private set; }

    // Lists of registered tickable objects
    readonly List<IPlayerTick> ticks = new();
    readonly List<IPlayerLateTick> lateTicks = new();

    // Buffers to avoid modifying the tick lists while iterating over them
    readonly List<IPlayerTick> tickBuffer = new();
    readonly List<IPlayerLateTick> lateTickBuffer = new();

    bool isTicking = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        GameLog.Log(TAG, "Awake() finished. Singleton set");
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnEnable()
    {
        GameStateManager.StateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.StateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameState state)
    {
        isTicking = state == GameState.Gameplay;
    }

    // Registers an object that implements IPlayerTick or IPlayerLateTick to be ticked every frame
    public void Register(object tickable)
    {
        bool registered = false;

        if (tickable is IPlayerTick tick && !ticks.Contains(tick))
        {
            ticks.Add(tick);
            registered = true;
        }

        if (tickable is IPlayerLateTick lateTick && !lateTicks.Contains(lateTick))
        {
            lateTicks.Add(lateTick);
            registered = true;
        }

        if (!registered)  
            GameLog.Warning(TAG, $"Tried to register non-tickable object: {tickable}");
    }

    // Unregisters an object that implements IPlayerTick or IPlayerLateTick from being ticked every frame
    public void Unregister(object tickable)
    {
        if (tickable is IPlayerTick tick)
            ticks.Remove(tick);

        if (tickable is IPlayerLateTick lateTick)
            lateTicks.Remove(lateTick);
    }

    // Calls Tick() on all registered IPlayerTick objects every frame
    void Update()
    {
        if (!isTicking) return;

        float dt = Time.deltaTime;

        tickBuffer.Clear();
        tickBuffer.AddRange(ticks);

        for (int i = 0; i < tickBuffer.Count; i++)
            tickBuffer[i].Tick(dt);
    }

    // Calls LateTick() on all registered IPlayerLateTick objects every frame
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

// Interface for objects that want to be ticked every frame by the PlayerTickSystem
public interface IPlayerTick
{
    void Tick(float dt);
}

// Interface for objects that want to be late-ticked every frame by the PlayerTickSystem
public interface IPlayerLateTick
{
    void LateTick(float dt);
}