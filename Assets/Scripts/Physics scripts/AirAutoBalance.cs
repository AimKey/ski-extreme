using UnityEngine;

public class AirAutoBalance : MonoBehaviour
{
    [Header("Auto Balance Settings")]
    [SerializeField] private float balanceSpeed = 120f; // degrees per second
    [SerializeField] private float lookAheadDistance = 20f; // distance to raycast for slope detection
    [SerializeField] private float minAngleDifference = 5f; // minimum angle difference to trigger balance
    [SerializeField] private LayerMask groundLayerMask = 1; // ground layer mask
    [SerializeField] private float forwardOffset = 2f; // how far forward to look based on velocity
    
    private Rigidbody2D rb;
    private PlayerController playerController;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
    }

    void FixedUpdate()
    {
        // Only auto-balance when airborne and not manually controlling
        if (ShouldAutoBalance())
        {
            AutoBalanceToSlope();
        }
    }
    
    private bool ShouldAutoBalance()
    {
        // Don't auto-balance if grounded, player lost, or boosting
        if (playerController.isGrounded || playerController.IsPlayerLost || playerController.isBoosting)
            return false;
            
        // Don't auto-balance if player is manually rotating
        bool isRotatingWithSpace = Input.GetKey(KeyCode.Space);
        bool isRotatingWithActions = playerController.rotateLeftAction.ReadValue<float>() != 0f || 
                                   playerController.rotateRightAction.ReadValue<float>() != 0f;
        
        return !isRotatingWithSpace && !isRotatingWithActions;
    }
    
    private void AutoBalanceToSlope()
    {
        // Calculate raycast origin with forward offset based on velocity
        Vector2 rayOrigin = transform.position;
        Vector2 velocity = rb.linearVelocity;
        
        if (Mathf.Abs(velocity.x) > 1f)
        {
            Vector2 forwardDir = new Vector2(Mathf.Sign(velocity.x) * forwardOffset, 0f);
            rayOrigin += forwardDir;
        }
        
        // Raycast down to find the slope
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, lookAheadDistance, groundLayerMask);
        
        if (hit.collider != null)
        {
            // Calculate slope angle from surface normal
            Vector2 surfaceNormal = hit.normal;
            float slopeAngle = Mathf.Atan2(surfaceNormal.y, surfaceNormal.x) * Mathf.Rad2Deg - 90f;
            
            // Get current and target rotations
            float currentRotation = rb.rotation;
            float targetRotation = slopeAngle;
            
            // Calculate the shortest rotation path
            float rotationDifference = Mathf.DeltaAngle(currentRotation, targetRotation);
            
            // Only balance if difference is significant
            if (Mathf.Abs(rotationDifference) > minAngleDifference)
            {
                // Apply smooth rotation towards target
                float maxRotationStep = balanceSpeed * Time.fixedDeltaTime;
                float rotationStep = Mathf.Sign(rotationDifference) * Mathf.Min(maxRotationStep, Mathf.Abs(rotationDifference));
                
                rb.MoveRotation(currentRotation + rotationStep);
            }
        }
    }
    
    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (rb == null) return;
        
        // Draw raycast
        Vector2 rayOrigin = transform.position;
        Vector2 velocity = rb.linearVelocity;
        
        if (Mathf.Abs(velocity.x) > 1f)
        {
            Vector2 forwardDir = new Vector2(Mathf.Sign(velocity.x) * forwardOffset, 0f);
            rayOrigin += forwardDir;
        }
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.down * lookAheadDistance);
        
        // Draw hit point if any
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, lookAheadDistance, groundLayerMask);
        if (hit.collider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hit.point, 0.2f);
            
            // Draw surface normal
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(hit.point, hit.point + hit.normal * 2f);
        }
    }
}
