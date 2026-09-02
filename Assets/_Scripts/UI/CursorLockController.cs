using UnityEngine;
using VContainer;

public sealed class CursorLockController : MonoBehaviour
{
    public static CursorLockController Instance { get; private set; }

    [SerializeField] private bool lockOnStart = true;
    [SerializeField] private bool hideCursorWhenLocked = true;

    public bool IsLocked { get; private set; }

    GameStateManager _gameStateManager;

    [Inject]
    void Construct(GameStateManager gameStateManager) {
        _gameStateManager = gameStateManager;
    }

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
        _gameStateManager.PauseChanged += OnPausedChanged;
        SetLocked(!_gameStateManager.IsPaused);
    }

    private void OnDisable() => _gameStateManager.PauseChanged -= OnPausedChanged;
    private void OnPausedChanged(bool isPaused) => SetLocked(!isPaused);

    private void Start()
    {
        if (lockOnStart)
            LockCursor();
        else
            UnlockCursor();
    }

    public void LockCursor()
    {
        IsLocked = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = !hideCursorWhenLocked;
    }

    public void UnlockCursor()
    {
        IsLocked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SetLocked(bool locked)
    {
        if (locked)
            LockCursor();
        else
            UnlockCursor();
    }
}