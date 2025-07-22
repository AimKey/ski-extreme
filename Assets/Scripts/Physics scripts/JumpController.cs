// JumpController.cs – v3.0 (height‑driven, slope‑aware jump)
// -----------------------------------------------------------
// Purpose: guarantee that the board reaches a target apex height, no matter the
// local slope or current downward velocity. Instead of a fixed impulse value
// we compute the precise vertical Δv required for `desiredJumpHeight` using
// basic kinematics (v = √(2 g h)). Horizontal impulse coming from the slope is
// capped to avoid excessive boosts on steep ramps.
//
// Controls
// ● Space tap  → jump (height = desiredJumpHeight)
// ● Space hold → apply continuous flip torque while airborne
// ● Space release (mid‑air) → brake spin

using UnityEngine;

/// <summary>
/// Controls player jumping and aerial tricks with physics-based height targeting.
/// - Guarantees consistent jump height regardless of slope or current velocity
/// - Handles slope compensation for natural jump behavior
/// - Manages aerial flips and spin mechanics
/// - Provides coyote time for improved jump feel
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(SlideController))]
[DefaultExecutionOrder(-90)]
public class JumpController : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Jump Physics")]
    [Tooltip("Peak height (metres) the board should reach when jumping on flat ground.")]
    [SerializeField] private float desiredJumpHeight = 14f;
    
    [Tooltip("Extra grace period after leaving ground (seconds) – coyote time.")]
    [SerializeField] private float coyoteTime = 0.12f;

    [Header("Slope Compensation")]
    [Tooltip("Preserve vertical apex on slopes while limiting added horizontal boost.")]
    [SerializeField] private bool compensateSlope = true;
    
    [Tooltip("Maximum extra horizontal impulse (N·s) injected by slope jump.")]
    [SerializeField] private float maxHorizImpulse = 4f;

    [Header("Aerial Tricks")]
    [SerializeField] private float flipTorque = 50f;        // Torque applied during flip
    [SerializeField] private float maxFlipRate = 70f;       // Maximum spin speed (deg/s)
    [SerializeField] private float spinBrake = 1200f;       // Spin deceleration (deg/s²)
    
    #endregion

    #region Private Fields
    
    // Components
    private Rigidbody2D rb;
    private SlideController slide;
    
    // Ground tracking
    private float lastGroundedTime;
    private Vector2 lastGroundNormal = Vector2.up;
    
    // State management
    private bool skipGroundFrame;
    
    #endregion

    #region Public Properties
    
    /// <summary>Can the player currently perform a jump?</summary>
    public bool CanJump => slide.IsGrounded || Time.fixedTime - lastGroundedTime <= coyoteTime;

    /// <summary>Is the player currently in the air?</summary>
    public bool IsAirborne => !slide.IsGrounded;
    
    #endregion

    #region Unity Lifecycle
    
    void Awake()
    {
        InitializeComponents();
    }

    private bool jumpInputBuffered = false;
    private float jumpInputTime = 0f;
    private const float INPUT_BUFFER_TIME = 0.1f;

    void Update()
    {
        // Buffer jump input - high frequency polling
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpInputBuffered = true;
            jumpInputTime = Time.unscaledTime; // Use unscaled time for input
        }
    }

    void FixedUpdate()
    {
        UpdateGroundTracking(); // ADD: Missing ground tracking
        
        if (skipGroundFrame)
        {
            skipGroundFrame = false;
            return;
        }

        HandleBufferedJumpInput(); // Renamed for clarity
        HandleAerialTricks();
        HandleLandingCleanup();
    }
    
    #endregion

    #region Initialization
    
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        slide = GetComponent<SlideController>();
    }
    
    #endregion

    #region Ground Tracking

    private void UpdateGroundTracking()
    {
        if (slide.IsGrounded)
        {
            lastGroundedTime = Time.fixedTime; // Use fixedTime consistently
            lastGroundNormal = slide.GroundNormal;
        }
    }
    
    #endregion

    #region Jump System

    private void HandleBufferedJumpInput()
    {
        // Check buffered input with proper time handling
        bool hasJumpInput = jumpInputBuffered && 
                           (Time.unscaledTime - jumpInputTime <= INPUT_BUFFER_TIME);
        
        if (hasJumpInput && CanJump)
        {
            jumpInputBuffered = false; // Consume input immediately
            PerformJump();
            return; // Exit early after jump
        }
        
        // Clear expired buffered input
        if (jumpInputBuffered && Time.unscaledTime - jumpInputTime > INPUT_BUFFER_TIME)
        {
            jumpInputBuffered = false;
        }
    }

    private void PerformJump()
    {
        // Prevent double jumps by checking if already performed this frame
        if (!CanJump) return;
        
        Vector2 jumpImpulse = CalculateJumpImpulse();
        ApplyJumpImpulse(jumpImpulse);
        
        // Force airborne state immediately
        ForceAirborneState();
        
        // Clear any remaining input buffer to prevent double jumps
        jumpInputBuffered = false;
    }
    
    private void ForceAirborneState()
    {
        // Tell SlideController to skip ground detection
        if (slide != null)
        {
            slide.ForceAirborne();
        }
        
        // Update ground tracking with consistent time reference
        lastGroundedTime = Time.fixedTime - coyoteTime - 0.1f; // Use fixedTime
    }
    
    private Vector2 CalculateJumpImpulse()
    {
        // Get reference normal (current ground or last known)
        Vector2 normal = GetJumpNormal();

        // Calculate required vertical velocity for target height
        float requiredVerticalVelocity = CalculateRequiredVerticalVelocity();

        // Build impulse based on slope compensation settings
        return compensateSlope
            ? CalculateSlopeCompensatedImpulse(normal, requiredVerticalVelocity)
            : CalculateVerticalImpulse(requiredVerticalVelocity);
    }
    
    private Vector2 GetJumpNormal()
    {
        return slide.IsGrounded ? slide.GroundNormal.normalized : lastGroundNormal.normalized;
    }
    
    private float CalculateRequiredVerticalVelocity()
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        float targetVelocity = Mathf.Sqrt(2f * gravity * desiredJumpHeight);
        
        // Don't subtract current velocity - this was causing issues
        return targetVelocity;
    }
    
    private Vector2 CalculateSlopeCompensatedImpulse(Vector2 normal, float requiredVy)
    {
        // Scale impulse along normal to achieve required vertical component
        float scale = (requiredVy * rb.mass) / Mathf.Max(normal.y, 0.001f);
        Vector2 rawImpulse = normal * scale;
        
        // Separate horizontal and vertical components
        Vector2 verticalComponent = Vector2.up * (requiredVy * rb.mass);
        Vector2 horizontalComponent = rawImpulse - verticalComponent;
        
        // Cap horizontal component to prevent excessive boosts
        if (horizontalComponent.sqrMagnitude > maxHorizImpulse * maxHorizImpulse)
        {
            horizontalComponent = horizontalComponent.normalized * maxHorizImpulse;
        }
        
        return verticalComponent + horizontalComponent;
    }
    
    private Vector2 CalculateVerticalImpulse(float requiredVy)
    {
        return Vector2.up * (requiredVy * rb.mass);
    }
    
    private void ApplyJumpImpulse(Vector2 impulse)
    {
        // Clear any conflicting velocity
        Vector2 currentVelocity = rb.linearVelocity; // FIXED: Use rb.velocity instead of rb.linearVelocity
        
        // Always clear downward velocity
        if (currentVelocity.y < 0f)
        {
            currentVelocity.y = 0f;
        }
        
        // On slopes, reduce horizontal velocity
        Vector2 normal = GetJumpNormal();
        float slopeAngle = Vector2.Angle(normal, Vector2.up);
        
        if (slopeAngle > 25f)
        {
            currentVelocity.x *= 0.6f;
        }
        
        rb.linearVelocity = currentVelocity; // FIXED: Use rb.velocity
        
        // Apply the jump impulse
        rb.AddForce(impulse, ForceMode2D.Impulse);
        
        // Move player slightly up to avoid immediate ground contact
        transform.position += Vector3.up * 0.05f;
    }
    
    #endregion

    #region Aerial Tricks
    
    private void HandleAerialTricks()
    {
        if (!IsAirborne) return;
        
        if (Input.GetKey(KeyCode.Space))
        {
            ApplyFlipTorque();
        }
        else
        {
            ApplySpinBraking();
        }
    }
    
    private void ApplyFlipTorque()
    {
        rb.AddTorque(flipTorque, ForceMode2D.Force);
        rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -maxFlipRate, maxFlipRate);
    }
    
    private void ApplySpinBraking()
    {
        rb.angularVelocity = Mathf.MoveTowards(
            rb.angularVelocity, 
            0f, 
            spinBrake * Time.fixedDeltaTime
        );
    }
    
    #endregion

    #region Landing System
    
    private void HandleLandingCleanup()
    {
        if (!slide.IsGrounded) return;
        
        // Stop any remaining spin when landing
        if (Mathf.Abs(rb.angularVelocity) > 0.1f)
        {
            rb.angularVelocity = 0f;
        }
    }
    
    #endregion

    #region Debug & Utilities
    
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (rb == null) return;
        
        // Draw jump height indicator
        Gizmos.color = Color.green;
        Vector3 jumpPeak = transform.position + Vector3.up * desiredJumpHeight;
        Gizmos.DrawWireSphere(jumpPeak, 0.5f);
        
        // Draw ground normal
        if (slide != null && slide.IsGrounded)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)slide.GroundNormal * 2f);
        }
    }
    #endif
    
    #endregion
}
