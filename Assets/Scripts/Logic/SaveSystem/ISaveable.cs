// Interface used for any instance that should be saved
public interface ISaveable
{
    string saveId { get; }
    object CaptureState();
    void RestoreState(object state);
    void ResetToDefaultState();
}
