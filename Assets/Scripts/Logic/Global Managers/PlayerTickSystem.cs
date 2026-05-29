using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class PlayerTickSystem : MonoBehaviour
{
    private const string TAG = "PlayerTickSystem";

    public static PlayerTickSystem Instance { get; private set; }

    private readonly List<IPlayerTick> ticks = new();
    private readonly List<IPlayerLateTick> lateTicks = new();

    private readonly List<IPlayerTick> tickBuffer = new();
    private readonly List<IPlayerLateTick> lateTickBuffer = new();

    public bool isTicking = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        GameLog.Log(TAG, "Awake() finished. Singleton set");
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
    
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

    public void Unregister(object tickable)
    {
        if (tickable is IPlayerTick tick)
            ticks.Remove(tick);

        if (tickable is IPlayerLateTick lateTick)
            lateTicks.Remove(lateTick);
    }

    private void Update()
    {
        if (!isTicking) return;

        float dt = Time.deltaTime;

        tickBuffer.Clear();
        tickBuffer.AddRange(ticks);

        for (int i = 0; i < tickBuffer.Count; i++)
            tickBuffer[i].Tick(dt);
    }

    private void LateUpdate()
    {
        if (!isTicking) return;

        float dt = Time.deltaTime;

        lateTickBuffer.Clear();
        lateTickBuffer.AddRange(lateTicks);

        for (int i = 0; i < lateTickBuffer.Count; i++)
            lateTickBuffer[i].LateTick(dt);
    }
}

public interface IPlayerTick
{
    void Tick(float dt);
}

public interface IPlayerLateTick
{
    void LateTick(float dt);
}