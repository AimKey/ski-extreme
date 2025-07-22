// AirSpinBalance.cs – v2.1 (compile‑clean, slope‑aware auto‑balance)
using UnityEngine;

/// <summary>
/// Provides automatic rotation balance while airborne to prepare for landing.
/// - Spins the player at a cruise rate when airborne
/// - Auto-balances to match upcoming slope angle
/// - Prevents excessive nose-down rotation
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(SlideController))]
[DefaultExecutionOrder(150)]
public class AirAutoBalance : MonoBehaviour
{
    #region Serialized Fields

    [Header("Cruise Spin")]
    [Tooltip("Degrees per second; negative = clockwise")]
    [SerializeField] private float cruiseRate = -90f;
    [Tooltip("Maximum nose-down angle allowed (degrees)")]
    [SerializeField] private float maxNoseDown = 60f;

    [Header("Slope Alignment")]
    [Tooltip("Distance to probe forward & down for landing surface")]
    [SerializeField] private float lookAhead = 3f;
    [Tooltip("Stop spinning when board-to-slope ≤ this many degrees")]
    [SerializeField] private float alignTolerance = 6f;
    [Tooltip("Speed of auto-balance rotation (degrees/second)")]
    [SerializeField] private float balanceSpeed = 180f;
    [Tooltip("Maximum ground angle to consider valid for auto-balance")]
    [SerializeField] private float maxGroundAngle = 65f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Stability")]
    [Tooltip("Seconds after leaving ground before spin starts")]
    [SerializeField] private float leaveGroundDelay = 0.08f;

    [Header("Recovery Settings")]
    [Tooltip("Minimum time to wait before auto-recovery starts")]
    [SerializeField] private float recoveryDelay = 0.3f;
    [Tooltip("Speed of recovery rotation when upside down")]
    [SerializeField] private float recoverySpeed = 120f;
    [Tooltip("Don't auto-recover if rotation difference is less than this")]
    [SerializeField] private float minRecoveryAngle = 45f;

    #endregion

    #region Private Fields

    private Rigidbody2D rb;
    private SlideController slide;
    private float airTimer;
    private float timeSinceSpaceReleased; // Track when space was last released
    private bool wasFlipping; // Track previous flipping state

    #endregion

    #region Unity Lifecycle

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        slide = GetComponent<SlideController>();
    }

    void FixedUpdate()
    {
        UpdateAirTimer();
        UpdateFlippingState();
        
        if (ShouldPerformAutoBalance())
        {
            PerformAutoBalance();
        }
    }

    #endregion

    #region Air Timer Management

    private void UpdateAirTimer()
    {
        bool grounded = slide.IsGrounded;
        airTimer = grounded ? 0f : airTimer + Time.fixedDeltaTime;
    }

    private void UpdateFlippingState()
    {
        bool currentlyFlipping = Input.GetKey(KeyCode.Space);
        
        // Track when space was released
        if (wasFlipping && !currentlyFlipping)
        {
            timeSinceSpaceReleased = 0f;
        }
        
        // Update timer if not flipping
        if (!currentlyFlipping)
        {
            timeSinceSpaceReleased += Time.fixedDeltaTime;
        }
        
        wasFlipping = currentlyFlipping;
    }

    #endregion

    #region Auto Balance Logic

    private bool ShouldPerformAutoBalance()
    {
        bool grounded = slide.IsGrounded;
        bool flipping = Input.GetKey(KeyCode.Space);
        bool hasBeenAirborneEnough = airTimer >= leaveGroundDelay;
        bool hasWaitedAfterFlip = timeSinceSpaceReleased >= recoveryDelay;
        bool rotationNotTooExtreme = !IsRotationTooExtreme(); // NEW: Check if rotation is manageable
        
        return !grounded && !flipping && hasBeenAirborneEnough && hasWaitedAfterFlip && rotationNotTooExtreme;
    }
    
    private bool IsRotationTooExtreme()
    {
        float currentRotation = NormalizeAngle(rb.rotation);
        float absRotation = Mathf.Abs(currentRotation);
        
        // Don't auto-balance if player is too close to upside down (within 30° of 180°)
        return absRotation > 150f; // If rotation is beyond 150°, it's too extreme
    }

    private void PerformAutoBalance()
    {
        // Check if we need recovery (upside down or significantly rotated)
        if (NeedsRecovery())
        {
            PerformRecovery();
            return;
        }

        // Normal auto-balance logic
        Vector2 probeDirection = CalculateProbeDirection();
        RaycastHit2D hit = Physics2D.Raycast(transform.position, probeDirection, lookAhead, groundMask);

        if (hit.collider != null && IsValidSlope(hit))
        {
            BalanceToSlope(hit);
        }
        else
        {
            RaycastHit2D fallbackHit = FindNearestSlope();

            if (fallbackHit.collider != null && IsValidSlope(fallbackHit))
            {
                BalanceToSlope(fallbackHit);
            }
            else
            {
                ApplyCruiseSpin();
            }
        }
    }

    private bool NeedsRecovery()
    {
        float currentRotation = NormalizeAngle(rb.rotation);
        float absRotation = Mathf.Abs(currentRotation);
        
        // Only recover if rotation is awkward but not too extreme
        // Recovery zone: between minRecoveryAngle and 150°
        return absRotation > minRecoveryAngle && absRotation <= 150f;
    }

    private void PerformRecovery()
    {
        float currentRotation = rb.rotation;
        
        // Don't recover if rotation is too extreme
        if (IsRotationTooExtreme())
        {
            Debug.Log("Rotation too extreme - skipping recovery");
            return;
        }
        
        // Determine the shortest path to upright (0°)
        float targetRotation = 0f;
        float rotationDifference = Mathf.DeltaAngle(currentRotation, targetRotation);
        
        // Calculate gentle recovery step
        float maxRecoveryStep = recoverySpeed * Time.fixedDeltaTime;
        float rotationStep = Mathf.Sign(rotationDifference) * Mathf.Min(maxRecoveryStep, Mathf.Abs(rotationDifference));
        
        // Apply recovery rotation
        float newRotation = currentRotation + rotationStep;
        rb.MoveRotation(newRotation);
        
        Debug.Log($"Recovery: Current {currentRotation:F1}°, Target {targetRotation:F1}°, Step {rotationStep:F1}°");
    }

    private float NormalizeAngle(float angle)
    {
        // Normalize angle to -180 to 180 range
        return Mathf.Repeat(angle + 180f, 360f) - 180f;
    }

    private Vector2 CalculateProbeDirection()
    {
        Vector2 velocity = rb.linearVelocity; // FIXED: Changed from rb.linearVelocity to rb.velocity
        
        if (Mathf.Abs(velocity.x) < 2f)
        {
            return Vector2.down;
        }
        
        Vector2 forward = new Vector2(Mathf.Sign(velocity.x), 0f);
        Vector2 probeDir = (Vector2.down + forward * 0.5f).normalized;
        
        return probeDir;
    }

    private void BalanceToSlope(RaycastHit2D hit)
    {
        float slopeRotation = Mathf.Atan2(hit.normal.y, hit.normal.x) * Mathf.Rad2Deg - 90f;
        float currentRotation = rb.rotation;
        float rotationDifference = Mathf.DeltaAngle(currentRotation, slopeRotation);
        
        if (Mathf.Abs(rotationDifference) <= alignTolerance)
        {
            return;
        }
        
        float maxRotationStep = balanceSpeed * Time.fixedDeltaTime;
        float rotationStep = Mathf.Sign(rotationDifference) * Mathf.Min(maxRotationStep, Mathf.Abs(rotationDifference));
        
        float targetRotation = currentRotation + rotationStep;
        targetRotation = ClampNoseDown(targetRotation);
        
        rb.MoveRotation(targetRotation);
    }

    private void ApplyCruiseSpin()
    {
        float currentRotation = rb.rotation;
        float targetRotation = currentRotation + cruiseRate * Time.fixedDeltaTime;
        
        targetRotation = ClampNoseDown(targetRotation);
        rb.MoveRotation(targetRotation);
    }

    private float ClampNoseDown(float rotation)
    {
        float normalizedRotation = NormalizeAngle(rotation);
        
        if (normalizedRotation < -maxNoseDown)
        {
            normalizedRotation = -maxNoseDown;
        }
        
        return normalizedRotation;
    }

    private RaycastHit2D FindNearestSlope()
    {
        // Try multiple probe directions to find a slope
        Vector2[] probeDirections = {
            Vector2.down,                                    // Straight down
            new Vector2(0.3f, -1f).normalized,              // Slightly forward-down
            new Vector2(-0.3f, -1f).normalized,             // Slightly backward-down
            new Vector2(0.7f, -1f).normalized,              // More forward-down
            new Vector2(-0.7f, -1f).normalized              // More backward-down
        };

        RaycastHit2D closestHit = default;
        float closestDistance = float.MaxValue;

        foreach (Vector2 direction in probeDirections)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, lookAhead, groundMask);

            if (hit.collider != null && hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closestHit = hit;
            }
        }

        return closestHit;
    }

    private bool IsValidSlope(RaycastHit2D hit)
    {
        float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
        return slopeAngle <= maxGroundAngle;
    }

    #endregion

    #region Debug

#if UNITY_EDITOR

    //
    private void OnDrawGizmosSelected()
    {
        if (rb == null) return;

        // Draw primary probe direction
        Vector2 probeDir = CalculateProbeDirection();
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)probeDir * lookAhead);

        // Draw current player rotation
        Gizmos.color = Color.green;
        float currentRot = rb.rotation * Mathf.Deg2Rad;
        Vector3 rotDir = new Vector3(Mathf.Cos(currentRot), Mathf.Sin(currentRot), 0);
        Gizmos.DrawLine(transform.position, transform.position + rotDir * 2f);

        // Draw all probe directions
        Vector2[] probeDirections = {
            Vector2.down,
            new Vector2(0.3f, -1f).normalized,
            new Vector2(-0.3f, -1f).normalized,
            new Vector2(0.7f, -1f).normalized,
            new Vector2(-0.7f, -1f).normalized
        };

        Gizmos.color = Color.cyan;
        foreach (Vector2 direction in probeDirections)
        {
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)direction * lookAhead * 0.5f);
        }

        // Draw detected slope
        RaycastHit2D hit = Physics2D.Raycast(transform.position, probeDir, lookAhead, groundMask);
        if (hit.collider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, hit.point);

            // Draw slope normal
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(hit.point, hit.point + hit.normal * 2f);

            // Draw target slope angle
            float slopeRotation = Mathf.Atan2(hit.normal.y, hit.normal.x) * Mathf.Rad2Deg - 90f;
            float slopeRot = slopeRotation * Mathf.Deg2Rad;
            Vector3 slopeDir = new Vector3(Mathf.Cos(slopeRot), Mathf.Sin(slopeRot), 0);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine((Vector3)hit.point, (Vector3)hit.point + slopeDir * 1.5f);
        }
    }
#endif

    #endregion
}
