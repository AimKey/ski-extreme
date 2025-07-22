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
    public bool CanJump => slide.IsGrounded || Time.time - lastGroundedTime <= coyoteTime;
    
    /// <summary>Is the player currently in the air?</summary>
    public bool IsAirborne => !slide.IsGrounded;
    
    #endregion

    #region Unity Lifecycle
    
    void Awake()
    {
        InitializeComponents();
    }

    void Update()
    {
        UpdateGroundTracking();
    }

    void FixedUpdate()
    {
        if (skipGroundFrame)
        {
            skipGroundFrame = false;
            return;
        }

        HandleJumpInput();
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
            lastGroundedTime = Time.time;
            lastGroundNormal = slide.GroundNormal;
        }
    }
    
    #endregion

    #region Jump System
    
    private void HandleJumpInput()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;
        if (!CanJump) return;
        
        PerformJump();
    }
    
    private void PerformJump()
    {
        Vector2 jumpImpulse = CalculateJumpImpulse();
        ApplyJumpImpulse(jumpImpulse);
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
        // Use kinematic equation: v = √(2gh) to find velocity needed for target height
        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        float targetVelocity = Mathf.Sqrt(2f * gravity * desiredJumpHeight);
        
        // Adjust for current downward velocity to maintain consistent apex
        return targetVelocity - Mathf.Max(rb.velocity.y, 0f);
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
        // Cancel downward velocity to avoid wasting impulse
        if (rb.velocity.y < 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
        
        // Apply jump impulse
        rb.AddForce(impulse, ForceMode2D.Impulse);
        
        // Skip ground forces for one frame to prevent interference
        skipGroundFrame = true;
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
