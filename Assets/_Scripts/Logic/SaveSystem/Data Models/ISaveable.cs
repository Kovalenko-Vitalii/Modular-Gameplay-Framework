namespace SaveSystem {
    /// <summary> Base Interface for objects that can be saved, restored, and reset to default state. </summary>
    public interface ISaveable {
        string saveId { get; }
        object CaptureState();
        void RestoreState(object state);
        void ResetToDefaultState();
    }
}