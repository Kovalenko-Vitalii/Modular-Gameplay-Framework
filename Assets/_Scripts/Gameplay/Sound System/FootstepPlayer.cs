using UnityEngine;
using VContainer;

// <summary>
// Class that handles playing footstep sounds based on the player's movement state and surface type
// </summary>
[DisallowMultipleComponent]
public sealed class FootstepPlayer : MonoBehaviour, ITick {
    [Header("Links")]
    [SerializeField] PlayerMovement _playerMovement;

    [Header("Step Distance")]
    [SerializeField, Min(0.01f)] float walkStepDistance = 1.75f;
    [SerializeField, Min(0.01f)] float sprintStepDistance = 2.05f;
    [SerializeField, Min(0.01f)] float crouchStepDistance = 1.35f;
    [SerializeField, Min(0.01f)] float minMoveSpeed = 0.25f;

    [Header("Landing")]
    [SerializeField, Min(0f)] float minLandingSpeed = 4f;
    [SerializeField, Min(0f)] float landingCooldown = 0.08f;

    float distanceAccumulator;
    bool wasGrounded;
    float lastAirVerticalVelocity;
    float lastLandingTime = -999f;

    SurfaceResolver _surfaceResolver;
    TickSystem _tickSystem;

    [Inject]
    void Construct(SurfaceResolver surfaceResolver, TickSystem tickSystem) {
        _surfaceResolver = surfaceResolver;
        _tickSystem = tickSystem;
    }

    void Awake() {
        if (_playerMovement == null) _playerMovement = GetComponent<PlayerMovement>();  
    }

    void OnEnable() {
        _tickSystem.Register(this);

        if (_playerMovement != null) {
            wasGrounded = _playerMovement.IsGrounded;
            _playerMovement.Jumped += PlayJump;
        }
    }

    void OnDisable() {
        _tickSystem.Unregister(this);

        if (_playerMovement != null) _playerMovement.Jumped -= PlayJump;
    }

    public void Tick(float dt) {
        if (_playerMovement == null) return;

        HandleLanding();

        if (!CanPlayFootsteps()) {
            distanceAccumulator = 0f;
            wasGrounded = _playerMovement.IsGrounded;

            if (!_playerMovement.IsGrounded)
                lastAirVerticalVelocity = _playerMovement.VerticalVelocity;

            return;
        }

        float horizontalSpeed = GetHorizontalSpeed();

        distanceAccumulator += horizontalSpeed * dt;

        float stepDistance = GetCurrentStepDistance();

        while (distanceAccumulator >= stepDistance) {
            distanceAccumulator -= stepDistance;

            if (_playerMovement.State == MovementState.Sprinting)
                PlayRun();
            else 
                PlayWalk();
        }

        wasGrounded = _playerMovement.IsGrounded;
    }

    bool CanPlayFootsteps() {
        if (!PlayerMovement.canMove)
            return false;

        if (!_playerMovement.IsGrounded)
            return false;

        if (_playerMovement.State == MovementState.Air)
            return false;

        return GetHorizontalSpeed() >= minMoveSpeed;
    }

    float GetHorizontalSpeed() {
        Vector3 velocity = _playerMovement.MoveDirection;
        velocity.y = 0f;

        return velocity.magnitude;
    }

    float GetCurrentStepDistance() {
        switch (_playerMovement.State) {
            case MovementState.Sprinting:
                return sprintStepDistance;

            case MovementState.Crouching:
                return crouchStepDistance;

            default:
                return walkStepDistance;
        }
    }

    Vector3 GetGroundCheckPosition() {
        Bounds bounds = _playerMovement.controller.bounds;
        return new Vector3(bounds.center.x, bounds.min.y + 0.05f, bounds.center.z);
    }

    void HandleLanding() {
        bool isGrounded = _playerMovement.IsGrounded;

        if (!wasGrounded && isGrounded) {
            float landingSpeed = Mathf.Abs(lastAirVerticalVelocity);

            if (landingSpeed >= minLandingSpeed && Time.time - lastLandingTime >= landingCooldown) {
                PlayLanding(landingSpeed);
                lastLandingTime = Time.time;
            }
        }

        if (!isGrounded)
            lastAirVerticalVelocity = _playerMovement.VerticalVelocity;
    }

    void PlayWalk() {
        SurfaceEntry surface = _surfaceResolver.GetSurfaceBelow(transform.position);
        if (surface == null) return;

        AkUnitySoundEngine.SetSwitch("Surface_Type", surface.SurfaceType.ToString(), gameObject);
        AkUnitySoundEngine.SetSwitch("Footstep_Type", "Walk", gameObject);
        AkUnitySoundEngine.PostEvent("Play_Footstep", gameObject);
    }

    void PlayRun() {
        SurfaceEntry surface = _surfaceResolver.GetSurfaceBelow(transform.position);
        if (surface == null) return;

        AkUnitySoundEngine.SetSwitch("Surface_Type", surface.SurfaceType.ToString(), gameObject);
        AkUnitySoundEngine.SetSwitch("Footstep_Type", "Run", gameObject);
        AkUnitySoundEngine.PostEvent("Play_Footstep", gameObject);
    }
    
    void PlayJump() {
        SurfaceEntry surface = _surfaceResolver.GetSurfaceBelow(transform.position);
        if (surface == null) return;

        AkUnitySoundEngine.SetSwitch("Surface_Type", surface.SurfaceType.ToString(), gameObject);
        AkUnitySoundEngine.SetSwitch("Footstep_Type", "Jump_Start", gameObject);
        AkUnitySoundEngine.PostEvent("Play_Footstep", gameObject);
    }

    void PlayLanding(float landingSpeed) {
        Vector3 groundPosition = GetGroundCheckPosition();
        SurfaceEntry surface = _surfaceResolver.GetSurfaceBelow(groundPosition);
        if (surface == null) return;

        AkUnitySoundEngine.SetSwitch("Surface_Type", surface.SurfaceType.ToString(), gameObject);
        AkUnitySoundEngine.SetSwitch("Footstep_Type", "Jump_Land", gameObject);
        AkUnitySoundEngine.PostEvent("Play_Footstep", gameObject);
    }

    
}