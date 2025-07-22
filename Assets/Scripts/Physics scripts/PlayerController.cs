using UnityEngine;

/// <summary>
/// Manages player collision detection and invincibility states.
/// - Detects head vs ground collisions for game over conditions
/// - Handles invincibility during boost phases
/// - Manages player and board collider interactions
/// - Controls magnetic mode for enhanced ground adherence
/// </summary>
[RequireComponent(typeof(SlideController))]
public class PlayerController : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundMask;      // assign "Ground" layer in Inspector
    
    [Header("Child References")]
    [SerializeField] private string playerColliderName = "Boarder_Top";     // Name of player head collider
    [SerializeField] private string boardColliderName = "Boarder_Bottom";   // Name of board collider
    
    [Header("Magnetic Mode")]
    [SerializeField] private float magneticForce = 100f;       // Force applied in magnetic mode
    [SerializeField] private float magneticDuration = 5f;      // How long magnetic mode lasts
    [SerializeField] private float magneticRange = 2f;         // Detection range for magnetic attraction
    [SerializeField] private float coinAttractionForce = 50f;  // Force applied to attract coins
    [SerializeField] private LayerMask coinMask = 1 << 6;      // Layer mask for coins
    
    #endregion

    #region Private Fields
    
    // Components
    private SlideController slide;
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Collider2D boardCollider;
    
    // State
    private bool isInvincible = false;
    private bool isMagneticMode = false;
    private float magneticTimer = 0f;
    
    #endregion

    #region Public Properties
    
    /// <summary>Is the player currently invincible?</summary>
    public bool IsInvincible => isInvincible;
    
    /// <summary>Reference to the player's head collider</summary>
    public Collider2D PlayerCollider => playerCollider;
    
    /// <summary>Reference to the board collider</summary>
    public Collider2D BoardCollider => boardCollider;
    
    /// <summary>Is magnetic mode currently active?</summary>
    public bool IsMagneticMode => isMagneticMode;
    
    /// <summary>Remaining time for magnetic mode</summary>
    public float MagneticTimeRemaining => magneticTimer;
    
    #endregion

    #region Unity Lifecycle
    
    void Awake()
    {
        InitializeComponents();
        FindChildColliders();
    }

    void Update()
    {
        UpdateInvincibilityState();
        UpdateMagneticMode();
    }

    void FixedUpdate()
    {
        if (isMagneticMode)
        {
            ApplyMagneticForces();
        }
    }
    
    #endregion

    #region Initialization
    
    private void InitializeComponents()
    {
        slide = GetComponent<SlideController>();
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void FindChildColliders()
    {
        // Find and assign colliders by child name
        Transform playerChild = transform.Find(playerColliderName);
        Transform boardChild = transform.Find(boardColliderName);

        if (playerChild != null) 
        {
            playerCollider = playerChild.GetComponent<Collider2D>();
        }
        else 
        {
            Debug.LogError($"{playerColliderName} child not found!");
        }

        if (boardChild != null) 
        {
            boardCollider = boardChild.GetComponent<Collider2D>();
        }
        else 
        {
            Debug.LogError($"{boardColliderName} child not found!");
        }
    }
    
    #endregion

    #region Invincibility System
    
    private void UpdateInvincibilityState()
    {
        bool shouldBeInvincible = slide.IsBoostActive;
        
        if (shouldBeInvincible && !isInvincible)
        {
            EnableInvincibility();
        }
        else if (!shouldBeInvincible && isInvincible)
        {
            DisableInvincibility();
        }
    }
    
    private void EnableInvincibility()
    {
        isInvincible = true;
        OnInvincibilityStart();
    }
    
    private void DisableInvincibility()
    {
        isInvincible = false;
        OnInvincibilityEnd();
    }
    
    #endregion

    #region Magnetic Mode System
    
    /// <summary>
    /// Activates magnetic mode for enhanced ground adherence
    /// </summary>
    public void EnterMagneticMode()
    {
        if (!isMagneticMode)
        {
            isMagneticMode = true;
            magneticTimer = magneticDuration;
            OnMagneticModeStart();
        }
        else
        {
            // Refresh timer if already active
            magneticTimer = magneticDuration;
        }
    }
    
    /// <summary>
    /// Manually exits magnetic mode
    /// </summary>
    public void ExitMagneticMode()
    {
        if (isMagneticMode)
        {
            isMagneticMode = false;
            magneticTimer = 0f;
            OnMagneticModeEnd();
        }
    }
    
    private void UpdateMagneticMode()
    {
        if (isMagneticMode)
        {
            magneticTimer -= Time.deltaTime;
            
            if (magneticTimer <= 0f)
            {
                ExitMagneticMode();
            }
        }
    }
    
    private void ApplyMagneticForces()
    {
        // Attract player toward ground (existing functionality)
        AttractToGround();
        
        // Attract coins toward player (new functionality)
        AttractCoins();
    }

    private void AttractToGround()
    {
        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position, 
            magneticRange, 
            Vector2.down, 
            magneticRange, 
            groundMask
        );
        
        if (hit.collider != null)
        {
            Vector2 forceDirection = (hit.point - (Vector2)transform.position).normalized;
            rb.AddForce(forceDirection * magneticForce * Time.fixedDeltaTime, ForceMode2D.Force);
        }
    }

    private void AttractCoins()
    {
        // Find all coins within magnetic range
        Collider2D[] coins = Physics2D.OverlapCircleAll(transform.position, magneticRange, coinMask);
        
        foreach (Collider2D coin in coins)
        {
            // Skip if coin doesn't have a rigidbody
            Rigidbody2D coinRb = coin.GetComponent<Rigidbody2D>();
            if (coinRb == null) continue;
            
            // Calculate force direction toward player
            Vector2 forceDirection = (transform.position - coin.transform.position).normalized;
            
            // Apply attraction force to coin
            coinRb.AddForce(forceDirection * coinAttractionForce * Time.fixedDeltaTime, ForceMode2D.Force);
        }
    }
    
    #endregion

    #region Collision Detection
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsGroundCollision(collision))
        {
            HandleGroundCollision(collision);
        }
    }
    
    private bool IsGroundCollision(Collision2D collision)
    {
        return ((1 << collision.collider.gameObject.layer) & groundMask) != 0;
    }
    
    private void HandleGroundCollision(Collision2D collision)
    {
        if (collision.otherCollider == playerCollider)
        {
            HandlePlayerHeadCollision();
        }
        else if (collision.otherCollider == boardCollider)
        {
            HandleBoardCollision();
        }
    }
    
    private void HandlePlayerHeadCollision()
    {
        Debug.Log("Player head hit ground!");
        
        if (!isInvincible)
        {
            Debug.Log("Game Over - Head collision!");
            TriggerGameOver();
        }
        else
        {
            Debug.Log("Head hit ground but player is invincible!");
        }
    }
    
    private void HandleBoardCollision()
    {
        Debug.Log("Board hit ground (normal landing)");
        // This is normal - the board should touch the ground
    }
    
    #endregion

    #region Game Over System
    
    private void TriggerGameOver()
    {
        FreezePlayerMovement();
        DisableControls();
        InvokeGameOverLogic();
    }
    
    private void FreezePlayerMovement()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.isKinematic = true;
    }
    
    private void DisableControls()
    {
        slide.enabled = false;
        enabled = false;
    }
    
    private void InvokeGameOverLogic()
    {
        // TODO: Implement game over logic
        // Example: GameManager.Instance.TriggerGameOver();
    }
    
    #endregion

    #region Event Handlers
    
    private void OnInvincibilityStart()
    {
        Debug.Log("Invincibility ON");
        // TODO: Add visual effects, sound, etc.
    }
    
    private void OnInvincibilityEnd()
    {
        Debug.Log("Invincibility OFF");
        // TODO: Remove visual effects, sound, etc.
    }
    
    private void OnMagneticModeStart()
    {
        Debug.Log("Magnetic Mode ON");
        // TODO: Add visual effects, sound, particles, etc.
    }
    
    private void OnMagneticModeEnd()
    {
        Debug.Log("Magnetic Mode OFF");
        // TODO: Remove visual effects, sound, particles, etc.
    }
    
    #endregion

    #region Debug & Utilities
    
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw collision detection visualization
        if (playerCollider != null)
        {
            Gizmos.color = isInvincible ? Color.green : Color.red;
            Gizmos.DrawWireCube(playerCollider.bounds.center, playerCollider.bounds.size);
        }
        
        if (boardCollider != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(boardCollider.bounds.center, boardCollider.bounds.size);
        }
        
        // Draw magnetic mode range
        if (isMagneticMode)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, magneticRange);
        }
    }
    #endif
    
    #endregion
}
