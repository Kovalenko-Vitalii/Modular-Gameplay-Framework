using UnityEngine;

/// <summary>
/// Optional base class for MonoBehaviours implementing ISaveable.
/// Handles registration with SaveRegistry automatically, so individual saveables
/// don't need to remember to call Register/Deregister themselves.
/// </summary>
public abstract class SaveableBehaviour : MonoBehaviour, ISaveable
{
    public abstract string saveId { get; }

    protected virtual void Awake() => SaveRegistry.Register(this);
    protected virtual void OnDestroy() => SaveRegistry.Deregister(this);
        
    public abstract object CaptureState();
    public abstract void RestoreState(object state);
    public abstract void ResetToDefaultState();
}