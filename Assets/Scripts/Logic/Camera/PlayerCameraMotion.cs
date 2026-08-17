using UnityEngine;

// <summary>
// Class that handles the camera motion for the player, including crouch offset and head bobbing
// </summary>
public sealed class PlayerCameraMotion : MonoBehaviour, ILateTick
{
    [Header("Links")]
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private CharacterController controller;

    [Header("Crouch Camera")]
    [SerializeField, Min(0f)] private float crouchCameraDrop = 0.75f;
    [SerializeField, Min(0f)] private float crouchSmoothness = 14f;

    [Header("Head Bob")]
    [SerializeField, Min(0f)] private float walkFrequency = 7f;
    [SerializeField, Min(0f)] private float sprintFrequency = 10f;

    [SerializeField, Min(0f)] private float walkHeight = 0.1f;
    [SerializeField, Min(0f)] private float sprintHeight = 0.11f;

    [SerializeField, Min(0f)] private float bobReturnSpeed = 10f;
    [SerializeField, Min(0f)] private float moveSpeedThreshold = 0.2f;

    private Vector3 _baseLocalPosition;
    private Vector3 _crouchOffset;
    private Vector3 _bobOffset;

    private float _bobTimer;

    private void Awake()
    {
        if (cameraRoot == null)
            cameraRoot = transform;

        if (movement == null)
            movement = GetComponentInParent<PlayerMovement>();

        if (controller == null)
            controller = GetComponentInParent<CharacterController>();

        _baseLocalPosition = cameraRoot.localPosition;
    }

    private void OnEnable()
    {
        TickSystem.Instance?.Register(this);
    }

    private void OnDisable()
    {
        TickSystem.Instance?.Unregister(this);
    }

    public void LateTick(float dt)
    {
        if (cameraRoot == null)
            return;

        UpdateCrouchOffset(dt);
        UpdateBobOffset(dt);

        cameraRoot.localPosition = _baseLocalPosition + _crouchOffset + _bobOffset;
    }

    private void UpdateCrouchOffset(float dt)
    {
        bool crouching = movement != null && movement.IsCrouching;

        Vector3 targetOffset = crouching
            ? Vector3.down * crouchCameraDrop
            : Vector3.zero;

        _crouchOffset = Vector3.Lerp(
            _crouchOffset,
            targetOffset,
            dt * crouchSmoothness
        );
    }

    private void UpdateBobOffset(float dt)
    {
        if (movement == null || controller == null)
        {
            ReturnBobToCenter(dt);
            return;
        }

        Vector3 velocity = movement.MoveDirection;
        velocity.y = 0f;

        bool grounded = movement.IsGrounded;
        bool moving = velocity.magnitude > moveSpeedThreshold;

        if (!grounded || !moving)
        {
            ReturnBobToCenter(dt);
            return;
        }

        bool sprinting = movement.IsSprinting;

        float frequency = sprinting ? sprintFrequency : walkFrequency;
        float height = sprinting ? sprintHeight : walkHeight;
        float maxSpeed = sprinting ? 7f : 4f;

        float speed01 = Mathf.InverseLerp(0f, maxSpeed, velocity.magnitude);

        float frequencyMultiplier = Mathf.Lerp(0.9f, 1.15f, speed01);
        float heightMultiplier = Mathf.Lerp(0.85f, 1.15f, speed01);

        _bobTimer += dt * frequency * frequencyMultiplier;

        float y = Mathf.Sin(_bobTimer) * height * heightMultiplier;
        float x = Mathf.Cos(_bobTimer * 0.5f) * height * 0.35f * heightMultiplier;

        _bobOffset = new Vector3(x, y, 0f);
    }

    private void ReturnBobToCenter(float dt)
    {
        _bobTimer = 0f;

        _bobOffset = Vector3.Lerp(
            _bobOffset,
            Vector3.zero,
            dt * bobReturnSpeed
        );
    }
}