namespace SaveSystem {
    /// <summary>
    /// Interface for objects that can be saved, restored, and reset to default state.
    /// 
    /// !!! Recommended to use SaveableBehaviour or a similar base class to handle registration automatically. !!!
    /// 
    /// </summary>
    public interface ISaveable
    {
        string saveId { get; }
        object CaptureState();
        void RestoreState(object state);
        void ResetToDefaultState();
    }
}