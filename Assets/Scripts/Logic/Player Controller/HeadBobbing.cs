using UnityEngine;

// This class is responsible for making realistic headbobbing for player's camera when moving
public class HeadBobbing : MonoBehaviour, IPlayerLateTick
{
    [Header("Links")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerMovement movement;

    [Header("Bobbing")]
    [SerializeField] private float walkFrequency = 7;
    [SerializeField] private float sprintFrequency = 10f;

    [SerializeField] private float walkHeight = 0.1f;
    [SerializeField] private float sprintHeight = 0.11f;

    [SerializeField] private float smoothReturnSpeed = 10f;

    [Header("When to bob")]
    [SerializeField] private float moveSpeedThreshold = 0.2f;

    private Vector3 initialLocalPos;
    private float timer;

    private void Awake()
    {
        ResolveReferences();
        initialLocalPos = cameraHolder.localPosition;
    }

    private void OnEnable() => PlayerTickSystem.Instance?.Register(this); 
    private void OnDisable() => PlayerTickSystem.Instance?.Unregister(this);  
    
    private void ResolveReferences()
    {
        if (controller == null)
            controller = GetComponentInParent<CharacterController>();

        if (movement == null)
            movement = GetComponentInParent<PlayerMovement>();

        if (cameraHolder == null)
            cameraHolder = transform;
    }

    private void Start()
    {
        initialLocalPos = cameraHolder.localPosition;

        if (controller == null)
            Debug.LogError("[HeadBobbing] controller is NULL. Assign CharacterController in Inspector.", this);

        if (movement == null)
            Debug.LogWarning("[HeadBobbing] movement is NULL. Assign PlayerMovement in Inspector.", this);

        if (PlayerTickSystem.Instance == null)
        {
            Debug.LogError("[HeadBobbing] PlayerTickSystem.Instance is null.", this);
            return;
        }
    }

    public void LateTick(float dt)
    {
        if (cameraHolder == null || controller == null)
            return;

        Vector3 vel = movement != null ? movement.MoveDirection : controller.velocity;
        vel.y = 0f;

        bool grounded = movement != null ? movement.IsGrounded : controller.isGrounded;
        bool moving = vel.magnitude > moveSpeedThreshold;

        if (!grounded || !moving)
        {
            ReturnToCenter(dt);
            return;
        }

        bool sprint = movement != null && movement.IsSprinting;

        float freq = sprint ? sprintFrequency : walkFrequency;
        float height = sprint ? sprintHeight : walkHeight;

        float maxSpeed = sprint ? 7f : 4f;
        float speed01 = Mathf.InverseLerp(0f, maxSpeed, vel.magnitude);

        float freqMul = Mathf.Lerp(0.9f, 1.15f, speed01);
        float heightMul = Mathf.Lerp(0.85f, 1.15f, speed01);

        timer += dt * freq * freqMul;

        float y = Mathf.Sin(timer) * height * heightMul;
        float x = Mathf.Cos(timer * 0.5f) * height * 0.35f * heightMul;

        cameraHolder.localPosition = initialLocalPos + new Vector3(x, y, 0f);
    }

    private void ReturnToCenter(float dt)
    {
        cameraHolder.localPosition = Vector3.Lerp(
            cameraHolder.localPosition,
            initialLocalPos,
            dt * smoothReturnSpeed
        );

        timer = 0f;
    }
}