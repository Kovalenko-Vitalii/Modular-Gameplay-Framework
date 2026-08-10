using UnityEngine;

// <summary>
// Class that handles playing footstep sounds based on the player's movement state and surface type
// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public sealed class FootstepPlayer : MonoBehaviour, IPlayerTick
{
    [Header("Links")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private CharacterController controller;
    [SerializeField] private SurfaceResolver surfaceResolver;
    [SerializeField] private AudioSource source;

    [Header("Step Distance")]
    [SerializeField, Min(0.01f)] private float walkStepDistance = 1.75f;
    [SerializeField, Min(0.01f)] private float sprintStepDistance = 2.05f;
    [SerializeField, Min(0.01f)] private float crouchStepDistance = 1.35f;
    [SerializeField, Min(0f)] private float minMoveSpeed = 0.25f;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float walkVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float sprintVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float crouchVolume = 0.45f;
    [SerializeField, Range(0f, 1f)] private float landVolume = 1f;

    [Header("Pitch")]
    [SerializeField] private Vector2 walkPitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField] private Vector2 sprintPitchRange = new Vector2(0.98f, 1.08f);
    [SerializeField] private Vector2 crouchPitchRange = new Vector2(0.88f, 0.98f);

    [SerializeField] private Vector2 jumpPitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField, Range(0f, 1f)] private float jumpVolume = 0.7f;

    [Header("Landing")]
    [SerializeField, Min(0f)] private float minLandingSpeed = 4f;
    [SerializeField, Min(0f)] private float landingCooldown = 0.08f;

    private float distanceAccumulator;
    private bool wasGrounded;
    private float lastAirVerticalVelocity;
    private float lastLandingTime = -999f;

    private AudioClip lastClip;

    private void Awake()
    {
        if (movement == null)
            movement = GetComponent<PlayerMovement>();

        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (surfaceResolver == null)
            surfaceResolver = GetComponent<SurfaceResolver>();

        if (source == null)
            source = GetComponent<AudioSource>();

        source.playOnAwake = false;
    }

    private void OnEnable()
    {
        PlayerTickSystem.Instance?.Register(this);

        if (movement != null)
        {
            wasGrounded = movement.IsGrounded;
            movement.Jumped += PlayJump;
        }
    }

    private void OnDisable()
    {
        PlayerTickSystem.Instance?.Unregister(this);

        if (movement != null)
            movement.Jumped -= PlayJump;
    }

    public void Tick(float dt)
    {
        if (movement == null || controller == null || surfaceResolver == null || source == null)
            return;

        HandleLanding();

        if (!CanPlayFootsteps())
        {
            distanceAccumulator = 0f;
            wasGrounded = movement.IsGrounded;

            if (!movement.IsGrounded)
                lastAirVerticalVelocity = movement.VerticalVelocity;

            return;
        }

        float horizontalSpeed = GetHorizontalSpeed();

        distanceAccumulator += horizontalSpeed * dt;

        float stepDistance = GetCurrentStepDistance();

        while (distanceAccumulator >= stepDistance)
        {
            distanceAccumulator -= stepDistance;
            PlayFootstep();
        }

        wasGrounded = movement.IsGrounded;
    }

    private bool CanPlayFootsteps()
    {
        if (!PlayerMovement.canMove)
            return false;

        if (!movement.IsGrounded)
            return false;

        if (movement.State == PlayerMovement.MovementState.Air)
            return false;

        return GetHorizontalSpeed() >= minMoveSpeed;
    }

    private float GetHorizontalSpeed()
    {
        Vector3 velocity = movement.MoveDirection;
        velocity.y = 0f;

        return velocity.magnitude;
    }

    private float GetCurrentStepDistance()
    {
        switch (movement.State)
        {
            case PlayerMovement.MovementState.Sprinting:
                return sprintStepDistance;

            case PlayerMovement.MovementState.Crouching:
                return crouchStepDistance;

            default:
                return walkStepDistance;
        }
    }

    private float GetCurrentVolume()
    {
        switch (movement.State)
        {
            case PlayerMovement.MovementState.Sprinting:
                return sprintVolume;

            case PlayerMovement.MovementState.Crouching:
                return crouchVolume;

            default:
                return walkVolume;
        }
    }

    private Vector2 GetCurrentPitchRange()
    {
        switch (movement.State)
        {
            case PlayerMovement.MovementState.Sprinting:
                return sprintPitchRange;

            case PlayerMovement.MovementState.Crouching:
                return crouchPitchRange;

            default:
                return walkPitchRange;
        }
    }

    private Vector3 GetGroundCheckPosition()
    {
        Bounds bounds = controller.bounds;
        return new Vector3(bounds.center.x, bounds.min.y + 0.05f, bounds.center.z);
    }

    private AudioClip GetCurrentFootstepClip(SurfaceEntry surface)
    {
        if (movement.State == PlayerMovement.MovementState.Sprinting)
            return surface.GetSprintFootstepClip(lastClip);

        return surface.GetWalkFootstepClip(lastClip);
    }

    private void PlayFootstep()
    {
        SurfaceEntry surface = surfaceResolver.GetSurfaceBelow(transform.position);

        if (surface == null)
            return;

        AudioClip clip = GetCurrentFootstepClip(surface);

        if (clip == null)
            return;

        lastClip = clip;

        Vector2 pitchRange = GetCurrentPitchRange();

        source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.PlayOneShot(clip, GetCurrentVolume());
    }

    private void HandleLanding()
    {
        bool isGrounded = movement.IsGrounded;

        if (!wasGrounded && isGrounded)
        {
            float landingSpeed = Mathf.Abs(lastAirVerticalVelocity);

            if (landingSpeed >= minLandingSpeed && Time.time - lastLandingTime >= landingCooldown)
            {
                PlayLanding(landingSpeed);
                lastLandingTime = Time.time;
            }
        }

        if (!isGrounded)
            lastAirVerticalVelocity = movement.VerticalVelocity;
    }

    private void PlayLanding(float landingSpeed)
    {
        Vector3 groundPosition = GetGroundCheckPosition();
        SurfaceEntry surface = surfaceResolver.GetSurfaceBelow(groundPosition);

        if (surface == null)
            return;

        AudioClip clip = surface.GetLandClip(lastClip);

        if (clip == null)
            return;

        lastClip = clip;

        float volumeMultiplier = Mathf.InverseLerp(minLandingSpeed, minLandingSpeed * 2.5f, landingSpeed);
        float finalVolume = Mathf.Lerp(landVolume * 0.65f, landVolume, volumeMultiplier);

        source.pitch = Random.Range(0.85f, 0.95f);
        source.PlayOneShot(clip, finalVolume);
    }

    private void PlayJump()
    {
        if (surfaceResolver == null || source == null)
            return;

        SurfaceEntry surface = surfaceResolver.GetSurfaceBelow(transform.position);

        if (surface == null)
            return;

        AudioClip clip = surface.GetJumpStartClip(lastClip);

        if (clip == null)
            return;

        lastClip = clip;

        source.pitch = Random.Range(jumpPitchRange.x, jumpPitchRange.y);
        source.PlayOneShot(clip, jumpVolume);
    }
}