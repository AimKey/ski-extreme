using UnityEngine;

/// <summary>
/// Controls player sliding physics with trick-based speed boosts.
/// - Handles ground detection and slope-adaptive movement
/// - Provides speed boosts when tricks are performed
/// - Manages both grounded and airborne physics
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
[DefaultExecutionOrder(-100)]
public class SlideController : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Trick Boost")]
    [SerializeField] private float boostMultiplier = 1.5f;      // Speed multiplier during boost
    [SerializeField] private float boostDuration = 2f;          // Boost duration in seconds
    [SerializeField] private float minSpeedBoostMul = 1.5f;     // Min speed boost multiplier

    [Header("Movement Forces")]
    [SerializeField] private float gravityBoost = 40f;          // Extra gravity when grounded
    [SerializeField] private float stickForce = 60f;            // Force to stick to ground
    [SerializeField] private float baseAccel = 4f;              // Base acceleration on flat ground

    [Header("Handling")]
    [SerializeField] private float turnSpeed = 140f;            // Turn speed in degrees/second
    [SerializeField] private float maxGroundAngle = 65f;        // Max angle considered as ground

    [Header("Speed Limits")]
    [SerializeField] private float minSlideSpeed = 25f;         // Minimum sliding speed
    [SerializeField] private float maxSlideSpeed = 50f;         // Maximum sliding speed
    [SerializeField, Range(0.5f, 3f)] 
    private float speedFactor = 1f;                             // Global speed multiplier

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundMask = ~0;         // Layers considered as ground
    
    #endregion

    #region Private Fields
    
    // Components
    private Rigidbody2D rb;
    
    // Boost system
    private float boostTimer;
    private float baseMinSlideSpeed;
    
    // Ground detection
    private readonly RaycastHit2D[] castBuffer = new RaycastHit2D[1];
    private Vector2 groundNormal = Vector2.up;
    private bool isGrounded;
    
    // Input
    private float yawInput;
    
    // Cached values
    private float minSlideSpeedSq;
    private float maxSlideSpeedSq;
    private Vector3[] rayOffsets;
    
    // Constants
    private static readonly Vector2 Down = Vector2.down;
    private static readonly Vector2 Zero2 = Vector2.zero;
    
    // Add this to your existing private fields
    private int forceAirborneFrames = 0;
    
    #endregion

    #region Public Properties
    
    /// <summary>Is the player currently touching the ground?</summary>
    public bool IsGrounded => isGrounded;
    
    /// <summary>Normal vector of the ground surface</summary>
    public Vector2 GroundNormal => groundNormal;
    
    /// <summary>Is trick boost currently active?</summary>
    public bool IsBoostActive => boostTimer > 0f;
    
    #endregion

    #region Public Methods

    /// <summary>
    /// Forces the controller to consider the player airborne for a few frames
    /// Used when jumping to prevent ground detection interference
    /// </summary>
    public void ForceAirborne()
    {
        isGrounded = false;
        forceAirborneFrames = 3; // Skip ground detection for 3 frames
    }

    #endregion

    #region Unity Lifecycle
    
    void Awake()
    {
        InitializeComponents();
        InitializePhysics();
        CacheInitialValues();
    }

    void OnValidate()
    {
        CacheSpeedSquares();
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        UpdateBoostTimer();
        DetectGround();
        
        if (isGrounded)
            HandleGroundedPhysics();
        else
            HandleAirbornePhysics();
    }
    
    #endregion

    #region Initialization
    
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void InitializePhysics()
    {
        rb.freezeRotation = false;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearDamping = 0f;
        Physics2D.queriesStartInColliders = false;
    }
    
    private void CacheInitialValues()
    {
        baseMinSlideSpeed = minSlideSpeed;
        rayOffsets = new Vector3[]
        {
            Zero2,
            Vector2.right * 0.45f,
            Vector2.left * 0.45f
        };
        CacheSpeedSquares();
    }
    
    private void CacheSpeedSquares()
    {
        minSlideSpeedSq = minSlideSpeed * minSlideSpeed;
        maxSlideSpeedSq = maxSlideSpeed * maxSlideSpeed;
    }
    
    #endregion

    #region Input Handling

    private void HandleInput()
    {
        // Force rightward movement only - ignore left input
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        yawInput = Mathf.Max(0, -horizontalInput); // Only allow right turns (positive yaw)
        
        // Alternative: Completely disable input for pure rightward sliding
        // yawInput = 0f;
    }

    #endregion

    #region Boost System
    
    /// <summary>
    /// Triggers a speed boost (called when tricks are performed)
    /// </summary>
    public void TriggerSpeedBoost()
    {
        boostTimer = Mathf.Max(boostTimer, boostDuration);
        minSlideSpeed = baseMinSlideSpeed * minSpeedBoostMul;
        CacheSpeedSquares();
    }
    
    private void UpdateBoostTimer()
    {
        if (boostTimer > 0f)
        {
            boostTimer -= Time.fixedDeltaTime;
            
            // Reset min speed when boost expires
            if (boostTimer <= 0f && minSlideSpeed != baseMinSlideSpeed)
            {
                minSlideSpeed = baseMinSlideSpeed;
                CacheSpeedSquares();
            }
        }
    }
    
    private float GetActiveSpeedFactor()
    {
        return IsBoostActive ? speedFactor * boostMultiplier : speedFactor;
    }
    
    private float GetActiveMaxSpeed()
    {
        return IsBoostActive ? maxSlideSpeed * boostMultiplier : maxSlideSpeed;
    }
    
    #endregion

    #region Physics Handling
    
    private void HandleGroundedPhysics()
    {
        ApplySlopeForces();
        ApplyRotation();
        ClampLinearSpeed(GetActiveMaxSpeed());
    }
    
    private void HandleAirbornePhysics()
    {
        rb.AddForce(Physics2D.gravity * (0.25f * speedFactor), ForceMode2D.Force);
        ClampLinearSpeed(GetActiveMaxSpeed());
    }
    
    private void ApplySlopeForces()
    {
        float slopeFactor = Mathf.Clamp01(Vector2.Angle(groundNormal, Vector2.up) / 90f);
        float activeSpeedFactor = GetActiveSpeedFactor();
        
        // Calculate downhill direction - ensure it's always rightward
        Vector2 downhillDirection = new Vector2(-groundNormal.y, groundNormal.x);
        
        // Force rightward movement: if downhill goes left, flip it to go right
        if (downhillDirection.x < 0)
        {
            downhillDirection.x = Mathf.Abs(downhillDirection.x);
        }
        
        float acceleration = (baseAccel + gravityBoost * slopeFactor) * activeSpeedFactor;
        
        // Apply forces
        rb.AddForce(downhillDirection * acceleration, ForceMode2D.Force);
        rb.AddForce(-groundNormal * stickForce * activeSpeedFactor, ForceMode2D.Force);
    }
    
    private void ApplyRotation()
    {
        float slopeDegrees = Mathf.Atan2(groundNormal.y, groundNormal.x) * Mathf.Rad2Deg - 90f;
        float yawStep = yawInput * turnSpeed * GetActiveSpeedFactor() * Time.fixedDeltaTime;
        
        float targetRotation = Mathf.LerpAngle(rb.rotation, slopeDegrees, 0.4f) + yawStep;
        rb.MoveRotation(targetRotation);
    }
    
    private void ClampLinearSpeed(float maxSpeed)
    {
        float currentSpeedSqr = rb.velocity.sqrMagnitude;
        
        // Enforce rightward movement
        Vector2 velocity = rb.velocity;
        if (velocity.x < 0)
        {
            velocity.x = Mathf.Abs(velocity.x); // Flip leftward velocity to rightward
        }
        
        // Enforce minimum speed
        if (currentSpeedSqr < minSlideSpeedSq && currentSpeedSqr > 1e-4f)
        {
            velocity = velocity.normalized * minSlideSpeed;
            // Ensure minimum rightward velocity
            if (velocity.x < minSlideSpeed * 0.5f)
            {
                velocity.x = minSlideSpeed * 0.5f;
            }
        }
        // Enforce maximum speed
        else if (currentSpeedSqr > maxSpeed * maxSpeed)
        {
            velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
        }
        
        rb.velocity = velocity;
    }
    
    #endregion

    #region Ground Detection
    
    private void DetectGround()
    {
        // Skip ground detection if forced airborne
        if (forceAirborneFrames > 0)
        {
            forceAirborneFrames--;
            isGrounded = false;
            return;
        }
        
        isGrounded = false;
        
        const float skinWidth = 0.05f;
        float castDistance = Mathf.Max(rb.velocity.magnitude * Time.fixedDeltaTime + skinWidth, 0.25f);
        
        // Try straight down cast
        if (PerformSweepCast(Down, castDistance, out RaycastHit2D hitInfo))
        {
            SetGroundInfo(hitInfo);
            return;
        }
        
        // Try forward-down cast
        Vector2 forwardDown = (Down + rb.velocity.normalized * 0.35f).normalized;
        if (PerformSweepCast(forwardDown, castDistance, out hitInfo))
        {
            SetGroundInfo(hitInfo);
            return;
        }
        
        // Fallback: circle cast
        var circleHit = Physics2D.CircleCast(transform.position, 0.45f, Down, castDistance, groundMask);
        if (IsValidGroundHit(circleHit))
        {
            SetGroundInfo(circleHit);
        }
    }
    
    private bool PerformSweepCast(Vector2 direction, float distance, out RaycastHit2D hit)
    {
        int hitCount = rb.Cast(direction, castBuffer, distance);
        hit = hitCount > 0 ? castBuffer[0] : default;
        return IsValidGroundHit(hit);
    }
    
    private bool IsValidGroundHit(RaycastHit2D hit)
    {
        return hit.collider != null && 
               hit.fraction > 0f && 
               Vector2.Angle(hit.normal, Vector2.up) <= maxGroundAngle;
    }
    
    private void SetGroundInfo(RaycastHit2D hit)
    {
        isGrounded = true;
        groundNormal = hit.normal;
    }
    
    #endregion
}
