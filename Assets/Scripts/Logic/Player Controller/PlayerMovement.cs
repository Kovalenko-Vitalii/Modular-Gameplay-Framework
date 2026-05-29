using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public sealed class PlayerMovement : MonoBehaviour
{
    [Header("Walking")]
    [SerializeField, Min(0f)] private float walkSpeed = 4f;
    [SerializeField, Min(0f)] private float sprintSpeed = 7f;
    [SerializeField, Min(0f)] private float backwardSpeed = 3f;

    [SerializeField, Min(0f)] private float speedUpSmoothness = 12f;
    [SerializeField, Min(0f)] private float speedDownSmoothness = 16f;

    [Header("Jump")]
    [SerializeField, Min(0f)] private float jumpHeight = 1.25f;

    [SerializeField, Min(0f)] private float coyoteTime = 0.1f; // Allows jumping shortly after leaving a walkable surface
    [SerializeField, Min(0f)] private float jumpBufferTime = 0.12f; // Allows jump input to be buffered shortly before landing on a walkable surface
    [SerializeField, Min(0f)] private float jumpCooldown = 0.08f;

    private float _lastWalkableGroundedTime = -999f;
    private float _lastJumpPressedTime = -999f;
    private float _lastJumpTime = -999f;

    [Header("Crouch")]
    [SerializeField, Min(0f)] private float crouchSpeed = 2.2f;
    [SerializeField, Min(0.1f)] private float crouchHeight = 1.2f;
    [SerializeField, Min(0f)] private float crouchSmoothness = 14f;

    [Tooltip("Transform камеры или camera holder, который надо опускать при приседе.")]
    [SerializeField] private Transform cameraRoot;

    [SerializeField, Min(0f)] private float crouchCameraDrop = 0.55f;
    [SerializeField] private LayerMask crouchObstructionMask = ~0;
    [SerializeField, Min(0f)] private float standCheckSkin = 0.02f;

    [Header("Gravity")]
    [SerializeField, Min(0f)] private float gravityForce = 25f;
    [SerializeField, Min(0f)] private float groundedStickForce = 4f;
    [SerializeField, Min(1f)] private float maxFallSpeed = 55f;

    [Header("Ground detection")]
    [SerializeField] private LayerMask whatIsGround = ~0;

    [SerializeField, Min(0.01f)] private float groundProbeDistance = 0.35f;

    [SerializeField, Min(0f)] private float groundProbeStartOffset = 0.05f;

    private float groundProbeRadiusScale = 1.1f; // Slightly larger than controller radius to better detect edges

    [Header("Slope Handling")]

    [SerializeField, Min(0f)] private float slideSpeed = 5f;
    [SerializeField, Min(0f)] private float slideAcceleration = 35f;
    
    [SerializeField, Range(0f, 5f)] private float slopeAngleTolerance = 1.5f; // Allow a small grace angle beyond the controller's slope limit to prevent jitter on edges

    [SerializeField, Range(55f, 89f)] private float wallLikeSlopeAngle = 68f; // Angles above this are treated as walls for sliding and detachment purposes

    [Tooltip("Минимальная горизонтальная скорость выхода с 70-80 градусных слоупов.")]
    [SerializeField, Min(0f)] private float minWallLikeSlideHorizontalSpeed = 2.5f;

    [Tooltip("Мягкое отталкивание от почти вертикальной поверхности, чтобы не залипать.")]
    [SerializeField, Min(0f)] private float wallDetachSpeed = 1.25f;

    [Tooltip("Сколько секунд помнить контакт со стеноподобным слоупом.")]
    [SerializeField, Min(0f)] private float wallContactMemory = 0.12f;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference crouchAction;

    [Header("References")]
    [SerializeField] private Transform orientation;

    private CharacterController controller;

    public enum MovementState
    {
        Walking,
        Sprinting,
        Crouching,
        Air
    }

    public MovementState State { get; private set; }

    public bool IsSprinting => State == MovementState.Sprinting;
    public float VerticalVelocity => _verticalVelocity;
    public bool IsGrounded => _ground.IsWalkable;
    public bool IsOnSteepSlope => _ground.IsSteep;
    public Vector3 MoveDirection => _moveDirection;
    public bool IsCrouching => _isCrouching;

    public static bool canMove = true;

    private float _defaultStepOffset;
    private float _currentSpeed;
    private float _horizontalInput;
    private float _verticalInput;
    private float _verticalVelocity;

    private Vector3 _moveDirection;
    private Vector3 _slideVelocity;

    private Vector3 _lastWallLikeNormal;
    private float _lastWallLikeContactTime = -999f;

    private GroundInfo _ground;
    private CollisionFlags _lastCollisionFlags;

    private const float MinSqrMagnitude = 0.0001f;

    private bool _wantsCrouch;
    private bool _isCrouching;

    private float _standingHeight;
    private Vector3 _standingCenter;
    private Vector3 _standingCameraLocalPosition;

    private readonly Collider[] _standCheckHits = new Collider[8];

    private struct GroundInfo
    {
        public bool HasHit;
        public bool IsWalkable;
        public bool IsSteep;
        public Vector3 Normal;
        public Vector3 Point;
        public float Distance;
        public float Angle;
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        controller.enableOverlapRecovery = true;

        _defaultStepOffset = controller.stepOffset;
        _currentSpeed = walkSpeed;

        _standingHeight = controller.height;
        _standingCenter = controller.center;

        if (cameraRoot == null)
            cameraRoot = orientation;

        if (cameraRoot != null)
            _standingCameraLocalPosition = cameraRoot.localPosition;

        crouchHeight = Mathf.Clamp(crouchHeight, controller.radius * 2f, _standingHeight);

        if (orientation == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
                orientation = cam.transform;
        }
    }

    private void OnEnable()
    {
        SetActionEnabled(moveAction, true);
        SetActionEnabled(jumpAction, true);
        SetActionEnabled(sprintAction, true);
        SetActionEnabled(crouchAction, true);
    }

    private void OnDisable()
    {
        SetActionEnabled(moveAction, false);
        SetActionEnabled(jumpAction, false);
        SetActionEnabled(sprintAction, false);
        SetActionEnabled(crouchAction, false);
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        ReadInput();
        UpdateCrouch(dt);
        BufferJumpInput();

        ProbeGround();
        UpdateGroundMemory();

        HandleState(dt);
        HandleJump();

        ApplyGravity(dt);
        Move(dt);
    }
    
    private static void SetActionEnabled(InputActionReference actionReference, bool enabled)
    {
        if (actionReference == null || actionReference.action == null)
            return;

        if (enabled)
            actionReference.action.Enable();
        else
            actionReference.action.Disable();
    }

    private void ReadInput()
    {
        if (!canMove)
        {
            _horizontalInput = 0f;
            _verticalInput = 0f;
            return;
        }

        Vector2 moveInput = moveAction != null && moveAction.action != null
            ? moveAction.action.ReadValue<Vector2>()
            : Vector2.zero;

        _horizontalInput = moveInput.x;
        _verticalInput = moveInput.y;

        _wantsCrouch =
            canMove &&
            crouchAction != null &&
            crouchAction.action != null &&
            crouchAction.action.IsPressed();
    }

    private void HandleState(float dt)
    {
        bool groundedOnWalkableSurface = _ground.IsWalkable && _verticalVelocity <= 0.01f;

        bool crouching = _isCrouching || _wantsCrouch;

        bool sprintKeyPressed =
            groundedOnWalkableSurface &&
            !crouching &&
            sprintAction != null &&
            sprintAction.action != null &&
            sprintAction.action.IsPressed() &&
            _verticalInput > 0f;

        if (!groundedOnWalkableSurface)
        {
            State = MovementState.Air;
            return;
        }

        float targetSpeed;
        float smoothness;

        if (crouching)
        {
            State = MovementState.Crouching;
            targetSpeed = crouchSpeed;
            smoothness = speedDownSmoothness;
        }
        else if (sprintKeyPressed)
        {
            State = MovementState.Sprinting;
            targetSpeed = sprintSpeed;
            smoothness = speedUpSmoothness;
        }
        else
        {
            State = MovementState.Walking;
            targetSpeed = _verticalInput < 0f ? backwardSpeed : walkSpeed;
            smoothness = speedDownSmoothness;
        }

        _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, dt * smoothness);
    }

    private void ApplyGravity(float dt)
    {
        if (_ground.IsWalkable && _verticalVelocity <= 0f)
        {
            _verticalVelocity = -groundedStickForce;
            return;
        }

        _verticalVelocity -= gravityForce * dt;
        _verticalVelocity = Mathf.Max(_verticalVelocity, -maxFallSpeed);
    }

    private void Move(float dt)
    {
        if (orientation == null)
        {
            Debug.LogError("[PlayerMovement] No orientation assigned. Player cannot move relative to camera yaw.", this);
            return;
        }

        Vector3 inputDirection = GetCameraRelativeInputDirection();

        _moveDirection = inputDirection * _currentSpeed;
        _moveDirection = RemoveUphillMovementOnSteepSlope(_moveDirection);

        transform.rotation = Quaternion.Euler(0f, orientation.eulerAngles.y, 0f);

        UpdateSlideVelocity(dt);
        UpdateStepOffset();

        Vector3 lateralVelocity = _moveDirection + new Vector3(_slideVelocity.x, 0f, _slideVelocity.z);
        lateralVelocity = CorrectVelocityAgainstWallLikeSlope(lateralVelocity);

        _lastCollisionFlags = controller.Move(lateralVelocity * dt);

        float finalVerticalVelocity = _verticalVelocity + Mathf.Min(0f, _slideVelocity.y);
        _lastCollisionFlags |= controller.Move(Vector3.up * finalVerticalVelocity * dt);

        if ((_lastCollisionFlags & CollisionFlags.Above) != 0 && _verticalVelocity > 0f)
            _verticalVelocity = 0f;

        if ((_lastCollisionFlags & CollisionFlags.Below) != 0 && _verticalVelocity < 0f)
            _verticalVelocity = -groundedStickForce;
    }

    private Vector3 GetCameraRelativeInputDirection()
    {
        Vector3 forward = orientation.forward;
        Vector3 right = orientation.right;

        forward.y = 0f;
        right.y = 0f;

        if (forward.sqrMagnitude > MinSqrMagnitude)
            forward.Normalize();

        if (right.sqrMagnitude > MinSqrMagnitude)
            right.Normalize();

        Vector3 direction = forward * _verticalInput + right * _horizontalInput;

        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        return direction;
    }

    private void UpdateStepOffset()
    {
        controller.stepOffset = _ground.IsWalkable && _verticalVelocity <= 0f
            ? _defaultStepOffset
            : 0f;
    }

    private void UpdateSlideVelocity(float dt)
    {
        Vector3 targetSlideVelocity = Vector3.zero;

        if (_ground.IsSteep)
        {
            Vector3 downhillDirection = Vector3.ProjectOnPlane(Vector3.down, _ground.Normal);

            if (downhillDirection.sqrMagnitude > MinSqrMagnitude)
            {
                downhillDirection.Normalize();

                targetSlideVelocity = downhillDirection * slideSpeed;

                if (_ground.Angle >= wallLikeSlopeAngle)
                {
                    Vector3 horizontalDownhill = new Vector3(
                        downhillDirection.x,
                        0f,
                        downhillDirection.z
                    );

                    if (horizontalDownhill.sqrMagnitude > MinSqrMagnitude)
                    {
                        horizontalDownhill.Normalize();

                        float horizontalSpeed = Mathf.Max(
                            minWallLikeSlideHorizontalSpeed,
                            wallDetachSpeed
                        );

                        targetSlideVelocity.x = horizontalDownhill.x * horizontalSpeed;
                        targetSlideVelocity.z = horizontalDownhill.z * horizontalSpeed;
                        targetSlideVelocity.y = 0f;
                    }
                    else if (HasRecentWallLikeContact())
                    {
                        targetSlideVelocity = _lastWallLikeNormal * wallDetachSpeed;
                    }
                }
            }
        }
        else if (!_ground.IsWalkable && HasRecentWallLikeContact())
        {
            targetSlideVelocity = _lastWallLikeNormal * wallDetachSpeed;
        }

        _slideVelocity = Vector3.MoveTowards(
            _slideVelocity,
            targetSlideVelocity,
            slideAcceleration * dt
        );
    }

    private Vector3 CorrectVelocityAgainstWallLikeSlope(Vector3 velocity)
    {
        if (!HasRecentWallLikeContact())
            return velocity;

        Vector3 wallNormal = _lastWallLikeNormal;

        float intoWallSpeed = Vector3.Dot(velocity, wallNormal);

        if (intoWallSpeed < 0f)
            velocity -= wallNormal * intoWallSpeed;

        if (_ground.IsSteep && _ground.Angle >= wallLikeSlopeAngle)
            velocity += wallNormal * wallDetachSpeed;

        return velocity;
    }

    private bool HasRecentWallLikeContact()
    {
        return Time.time - _lastWallLikeContactTime <= wallContactMemory &&
               _lastWallLikeNormal.sqrMagnitude > MinSqrMagnitude;
    }

    private Vector3 RemoveUphillMovementOnSteepSlope(Vector3 velocity)
    {
        if (!_ground.IsSteep)
            return velocity;

        Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, _ground.Normal);
        downhill.y = 0f;

        if (downhill.sqrMagnitude <= MinSqrMagnitude)
            return velocity;

        downhill.Normalize();

        Vector3 uphill = -downhill;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);

        float uphillAmount = Vector3.Dot(horizontalVelocity, uphill);

        if (uphillAmount <= 0f)
            return velocity;

        Vector3 correctedHorizontal = horizontalVelocity - uphill * uphillAmount;

        velocity.x = correctedHorizontal.x;
        velocity.z = correctedHorizontal.z;

        return velocity;
    }

    private void ProbeGround()
    {
        _ground = default;

        Vector3 bottomSphereCenter = GetBottomSphereCenter();
        float probeRadius = GetGroundProbeRadius();

        float sphereCastDistance =
            groundProbeStartOffset +
            groundProbeDistance +
            Mathf.Max(0f, controller.radius - probeRadius);

        float rayDistance =
            groundProbeStartOffset +
            controller.radius +
            groundProbeDistance;

        Vector3 sphereOrigin = bottomSphereCenter + Vector3.up * groundProbeStartOffset;
        Vector3 rayOrigin = bottomSphereCenter + Vector3.up * groundProbeStartOffset;

        bool hasWalkable = false;
        bool hasSteep = false;

        GroundInfo bestWalkable = default;
        GroundInfo bestSteep = default;

        float bestWalkableScore = float.NegativeInfinity;
        float bestSteepDistance = float.PositiveInfinity;

        if (Physics.SphereCast(
                sphereOrigin,
                probeRadius,
                Vector3.down,
                out RaycastHit sphereHit,
                sphereCastDistance,
                whatIsGround,
                QueryTriggerInteraction.Ignore))
        {
            CollectGroundCandidate(
                sphereHit,
                ref hasWalkable,
                ref bestWalkable,
                ref bestWalkableScore,
                ref hasSteep,
                ref bestSteep,
                ref bestSteepDistance
            );
        }

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0f;
        right.y = 0f;

        if (forward.sqrMagnitude <= MinSqrMagnitude)
            forward = Vector3.forward;
        else
            forward.Normalize();

        if (right.sqrMagnitude <= MinSqrMagnitude)
            right = Vector3.right;
        else
            right.Normalize();

        float ringOffset = Mathf.Max(0.01f, (controller.radius - controller.skinWidth) * 0.6f);

        ProbeRay(
            rayOrigin,
            rayDistance,
            ref hasWalkable,
            ref bestWalkable,
            ref bestWalkableScore,
            ref hasSteep,
            ref bestSteep,
            ref bestSteepDistance
        );

        ProbeRay(
            rayOrigin + forward * ringOffset,
            rayDistance,
            ref hasWalkable,
            ref bestWalkable,
            ref bestWalkableScore,
            ref hasSteep,
            ref bestSteep,
            ref bestSteepDistance
        );

        ProbeRay(
            rayOrigin - forward * ringOffset,
            rayDistance,
            ref hasWalkable,
            ref bestWalkable,
            ref bestWalkableScore,
            ref hasSteep,
            ref bestSteep,
            ref bestSteepDistance
        );

        ProbeRay(
            rayOrigin + right * ringOffset,
            rayDistance,
            ref hasWalkable,
            ref bestWalkable,
            ref bestWalkableScore,
            ref hasSteep,
            ref bestSteep,
            ref bestSteepDistance
        );

        ProbeRay(
            rayOrigin - right * ringOffset,
            rayDistance,
            ref hasWalkable,
            ref bestWalkable,
            ref bestWalkableScore,
            ref hasSteep,
            ref bestSteep,
            ref bestSteepDistance
        );

        if (hasWalkable)
            _ground = bestWalkable;
        else if (hasSteep)
            _ground = bestSteep;
    }

    private void ProbeRay(
        Vector3 origin,
        float distance,
        ref bool hasWalkable,
        ref GroundInfo bestWalkable,
        ref float bestWalkableScore,
        ref bool hasSteep,
        ref GroundInfo bestSteep,
        ref float bestSteepDistance)
    {
        if (!Physics.Raycast(
                origin,
                Vector3.down,
                out RaycastHit hit,
                distance,
                whatIsGround,
                QueryTriggerInteraction.Ignore))
        {
            return;
        }

        CollectGroundCandidate(
            hit,
            ref hasWalkable,
            ref bestWalkable,
            ref bestWalkableScore,
            ref hasSteep,
            ref bestSteep,
            ref bestSteepDistance
        );
    }

    private void CollectGroundCandidate(
        RaycastHit hit,
        ref bool hasWalkable,
        ref GroundInfo bestWalkable,
        ref float bestWalkableScore,
        ref bool hasSteep,
        ref GroundInfo bestSteep,
        ref float bestSteepDistance)
    {
        if (hit.collider == null)
            return;

        if (hit.normal.y <= 0f)
            return;

        float angle = Vector3.Angle(hit.normal, Vector3.up);
        bool isWalkable = angle <= controller.slopeLimit + slopeAngleTolerance;

        GroundInfo info = new GroundInfo
        {
            HasHit = true,
            IsWalkable = isWalkable,
            IsSteep = !isWalkable,
            Normal = hit.normal,
            Point = hit.point,
            Distance = hit.distance,
            Angle = angle
        };

        if (isWalkable)
        {
            float score = hit.normal.y * 10f - hit.distance;

            if (!hasWalkable || score > bestWalkableScore)
            {
                hasWalkable = true;
                bestWalkable = info;
                bestWalkableScore = score;
            }

            return;
        }

        if (!hasSteep || hit.distance < bestSteepDistance)
        {
            hasSteep = true;
            bestSteep = info;
            bestSteepDistance = hit.distance;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider == null)
            return;

        if (!IsInLayerMask(hit.collider.gameObject.layer, whatIsGround))
            return;

        if (hit.normal.y < -0.01f)
            return;

        float angle = Vector3.Angle(hit.normal, Vector3.up);

        if (angle < wallLikeSlopeAngle)
            return;

        Vector3 planarNormal = new Vector3(hit.normal.x, 0f, hit.normal.z);

        if (planarNormal.sqrMagnitude <= MinSqrMagnitude)
            return;

        _lastWallLikeNormal = planarNormal.normalized;
        _lastWallLikeContactTime = Time.time;
    }

    private Vector3 GetBottomSphereCenter()
    {
        Vector3 worldCenter = transform.TransformPoint(controller.center);
        float halfHeight = Mathf.Max(controller.height * 0.5f, controller.radius);
        return worldCenter + Vector3.down * (halfHeight - controller.radius);
    }

    private float GetGroundProbeRadius()
    {
        float radius = controller.radius * groundProbeRadiusScale - controller.skinWidth;
        return Mathf.Max(0.01f, radius);
    }

    private static bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    private void BufferJumpInput()
    {
        if (!canMove)
        {
            _lastJumpPressedTime = -999f;
            return;
        }

        if (jumpAction == null || jumpAction.action == null)
            return;

        if (jumpAction.action.WasPressedThisFrame())
            _lastJumpPressedTime = Time.time;
    }

    private void UpdateGroundMemory()
    {
        // Обновляем grounded memory только когда стоим на нормальной поверхности
        // и не летим вверх после прыжка.
        if (_ground.IsWalkable && _verticalVelocity <= 0.01f)
            _lastWalkableGroundedTime = Time.time;
    }

    private void HandleJump()
    {
        if (!HasBufferedJump())
            return;

        if (!CanJump())
            return;

        ConsumeJumpInput();

        _verticalVelocity = CalculateJumpVelocity();

        // Отключаем stepOffset на кадр прыжка, чтобы CharacterController
        // не пытался "прилипнуть" к ступеньке/краю во время старта прыжка.
        controller.stepOffset = 0f;

        // На всякий случай убираем вертикальную часть slide.
        if (_slideVelocity.y < 0f)
            _slideVelocity.y = 0f;

        State = MovementState.Air;
    }

    private bool HasBufferedJump()
    {
        return Time.time - _lastJumpPressedTime <= jumpBufferTime;
    }

    private bool CanJump()
    {
        if (!canMove)
            return false;

        if (Time.time - _lastJumpTime < jumpCooldown)
            return false;

        // Нельзя прыгать со слишком крутого слоупа.
        if (_ground.IsSteep)
            return false;

        // Запрещаем повторный прыжок, пока персонаж уже летит вверх.
        if (_verticalVelocity > 0.01f)
            return false;

        bool hasGround = _ground.IsWalkable;
        bool hasCoyoteGround = Time.time - _lastWalkableGroundedTime <= coyoteTime;

        return hasGround || hasCoyoteGround;
    }

    private void ConsumeJumpInput()
    {
        _lastJumpPressedTime = -999f;
        _lastJumpTime = Time.time;

        // Чтобы coyote time не дал второй прыжок сразу после первого.
        _lastWalkableGroundedTime = -999f;
    }

    private float CalculateJumpVelocity()
    {
        return Mathf.Sqrt(2f * gravityForce * jumpHeight);
    }

    private void UpdateCrouch(float dt)
    {
        bool wantsToStand = !_wantsCrouch;
        bool blockedFromStanding = _isCrouching && wantsToStand && !CanStand();

        bool shouldCrouch = _wantsCrouch || blockedFromStanding;
        _isCrouching = shouldCrouch;

        float targetHeight = shouldCrouch ? crouchHeight : _standingHeight;
        Vector3 targetCenter = shouldCrouch ? GetCrouchedCenter() : _standingCenter;

        controller.height = Mathf.Lerp(controller.height, targetHeight, dt * crouchSmoothness);
        controller.center = Vector3.Lerp(controller.center, targetCenter, dt * crouchSmoothness);

        if (Mathf.Abs(controller.height - targetHeight) < 0.005f)
            controller.height = targetHeight;

        if ((controller.center - targetCenter).sqrMagnitude < 0.000025f)
            controller.center = targetCenter;

        UpdateCrouchCamera(dt, shouldCrouch);
    }

    private Vector3 GetCrouchedCenter()
    {
        float heightDifference = _standingHeight - crouchHeight;
        return _standingCenter + Vector3.down * (heightDifference * 0.5f);
    }

    private void UpdateCrouchCamera(float dt, bool crouched)
    {
        if (cameraRoot == null)
            return;

        Vector3 targetLocalPosition = _standingCameraLocalPosition;

        if (crouched)
            targetLocalPosition.y -= crouchCameraDrop;

        cameraRoot.localPosition = Vector3.Lerp(
            cameraRoot.localPosition,
            targetLocalPosition,
            dt * crouchSmoothness
        );
    }

    private bool CanStand()
    {
        GetCapsulePoints(
            _standingHeight,
            _standingCenter,
            out Vector3 bottom,
            out Vector3 top
        );

        float checkRadius = Mathf.Max(0.01f, controller.radius - standCheckSkin);

        int hitCount = Physics.OverlapCapsuleNonAlloc(
            bottom,
            top,
            checkRadius,
            _standCheckHits,
            crouchObstructionMask,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = _standCheckHits[i];

            if (hit == null)
                continue;

            if (hit == controller)
                continue;

            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                continue;

            return false;
        }

        return true;
    }

    private void GetCapsulePoints(
        float height,
        Vector3 center,
        out Vector3 bottom,
        out Vector3 top)
    {
        float radius = controller.radius;
        float halfHeight = Mathf.Max(height * 0.5f, radius);

        Vector3 worldCenter = transform.TransformPoint(center);
        Vector3 offset = transform.up * Mathf.Max(0f, halfHeight - radius);

        bottom = worldCenter - offset;
        top = worldCenter + offset;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        walkSpeed = Mathf.Max(0f, walkSpeed);
        sprintSpeed = Mathf.Max(0f, sprintSpeed);
        backwardSpeed = Mathf.Max(0f, backwardSpeed);

        speedUpSmoothness = Mathf.Max(0f, speedUpSmoothness);
        speedDownSmoothness = Mathf.Max(0f, speedDownSmoothness);

        gravityForce = Mathf.Max(0f, gravityForce);
        groundedStickForce = Mathf.Max(0f, groundedStickForce);
        maxFallSpeed = Mathf.Max(1f, maxFallSpeed);

        groundProbeDistance = Mathf.Max(0.01f, groundProbeDistance);
        groundProbeStartOffset = Mathf.Max(0f, groundProbeStartOffset);

        slideSpeed = Mathf.Max(0f, slideSpeed);
        slideAcceleration = Mathf.Max(0f, slideAcceleration);

        wallLikeSlopeAngle = Mathf.Clamp(wallLikeSlopeAngle, 55f, 89f);
        minWallLikeSlideHorizontalSpeed = Mathf.Max(0f, minWallLikeSlideHorizontalSpeed);
        wallDetachSpeed = Mathf.Max(0f, wallDetachSpeed);
        wallContactMemory = Mathf.Max(0f, wallContactMemory);

        jumpHeight = Mathf.Max(0f, jumpHeight);
        coyoteTime = Mathf.Max(0f, coyoteTime);
        jumpBufferTime = Mathf.Max(0f, jumpBufferTime);
        jumpCooldown = Mathf.Max(0f, jumpCooldown);
    }
#endif
}