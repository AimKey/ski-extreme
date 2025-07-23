using Assets.Scripts.Constants;
using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public bool isGrounded;
    [SerializeField] public Rigidbody2D rb;

    // Rotation related variables
    [SerializeField] private float rotationSpeed = 360; // Speed of rotation in degrees per second
    public InputAction rotateRightAction;
    public InputAction rotateLeftAction;

    // Jumping related variables
    public float jumpForce = 10f;
    private bool bufferJump = false;
    [SerializeField] private float bufferTime = 0.2f; // Time in seconds to buffer the jump
    private float bufferRemainingTime;


    // Particle prefabs
    [SerializeField] private ParticleSystem driftingParticlePrefab;
    [SerializeField] private GameObject deadParticlePrefab;
    [SerializeField] private ParticleSystem speedBoostParticlePrefab;

    // Trick related variables
    private float RotatedDegree = 0;
    
    // 360 detection variables
    [Header("360 Trick Settings")]
    [SerializeField] private float rotation360Tolerance = 30f; // Tolerance for 360 detection (±30 degrees)
    private float accumulated360Rotations = 0f; // Track accumulated rotations for 360 detection

    // Boost related variables
    [SerializeField] private float boostDuration = 2f;
    [SerializeField] private float boostMultiplier = 2f;
    [SerializeField] private AnimationCurve boostCurve; // ease-in-out curve
    private bool didFlip = false;
    private bool did360 = false; // Track if player performed 360
    public bool isBoosting = false;
    private float boostTimer;
    private TerrainManager terrainManager;

    // Magnetic mode variables
    [Header("Magnetic Mode Settings")]
    [SerializeField] private float magneticRange = 30f; // Range to attract coins
    [SerializeField] private float magneticForce = 4000f; // Force to pull coins
    public bool isMagneticMode = false;
    private float magneticTimer;

    // Child collider references
    [SerializeField] private Collider2D GameOverCollider;

    public bool IsPlayerLost = false;

    // Instance var
    public static PlayerController Instance { get; private set; }

    // Sound clips
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip crashSound;
    [SerializeField] private AudioClip trickPerformedSound;

    // Surface normal tst
    private Vector2 surfaceNormal = Vector2.up;

    // Camera zoom effect
    private CameraZoomEffect cameraZoomEffect;

    [Header("Player Animation")]
    // Player animation
    public Animator animator;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        rb = rb.GetComponent<Rigidbody2D>();
        bufferRemainingTime = bufferTime;

        // Initialize input actions
        rotateRightAction.Enable();
        rotateLeftAction.Enable();
        terrainManager = TerrainManager.Instance;
        // Init sound
        audioSource = GetComponent<AudioSource>();

        // Init camera zoom effect
        cameraZoomEffect = CameraZoomEffect.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        DecreaseJumpBufferTime();

    }

    private void FixedUpdate()
    {
        HandlePlayerRotation();
        HandleBoostPlayer();
        HandleMagneticMode();
        ApplyConstantForce();
    }

    private void ApplyConstantForce()
    {
        //rb.AddForce(-surfaceNormal * 300f, ForceMode2D.Force);
    }

    private void HandlePlayerRotation()
    {
        // If on the ground don't allow rotation
        if (isGrounded)
        {
            return;
        }

        float rotationAmount = 0f;

        // Check for space key input (counter-clockwise rotation)
        if (Input.GetKey(KeyCode.Space))
        {
            rotationAmount = -rotationSpeed * Time.deltaTime; // Negative for counter-clockwise
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            TriggerPermanentMagneticMode(); // Press M for permanent magnetic mode
        }

        // else
        // {
        //     // Apply rotation based on left/right input when not holding space
        //     float rotationInput = rotateLeftAction.ReadValue<float>() - rotateRightAction.ReadValue<float>();
        //     if (rotationInput != 0f)
        //     {
        //         rotationAmount = rotationInput * rotationSpeed * Time.deltaTime;
        //     }
        // }

        // Apply the rotation
        if (rotationAmount != 0f)
        {
            rb.MoveRotation(rb.rotation + rotationAmount);
        }

        // Handle player performing tricks related to rotations
        TrickHandler(rotationAmount);
    }

    private void TrickHandler(float rotationAmount)
    {
        RotatedDegree += rotationAmount;
        accumulated360Rotations += rotationAmount;

        // Check for 360-degree rotation (with tolerance)
        if (Mathf.Abs(accumulated360Rotations) >= (360f - rotation360Tolerance) && !isGrounded)
        {
            Debug.Log($"360 detected! Rotation: {accumulated360Rotations}");
            // Mark that player performed 360, boost will activate on landing
            did360 = true;
            GameManager.Instance.IncreaseScoreFromPlayerTrick("360_Spin");
            audioSource.PlayOneShot(trickPerformedSound);
            accumulated360Rotations = 0f; // Reset 360 counter
        }

        // Check for regular flips (300 degrees)
        if (Math.Abs(RotatedDegree) >= 300 && !isGrounded)
        {
            if (RotatedDegree <= -300)
            {
                didFlip = true;
                GameManager.Instance.IncreaseScoreFromPlayerTrick(GameConstants.FrontFlip);
                audioSource.PlayOneShot(trickPerformedSound);
            }
            else if (RotatedDegree >= 300)
            {
                didFlip = true;
                GameManager.Instance.IncreaseScoreFromPlayerTrick(GameConstants.BackFlip);
                audioSource.PlayOneShot(trickPerformedSound);
            }

            // Reset flip counter
            RotatedDegree = 0f;
        }
    }

    // Method to trigger boost mode with custom duration
    public void TriggerBoostMode(float duration)
    {
        isBoosting = true;
        boostTimer = duration;
        speedBoostParticlePrefab.Play();
        Debug.Log($"Boost mode activated for {duration} seconds!");
    }

    // Method to trigger magnetic mode with custom duration
    public void TriggerMagneticMode(float duration)
    {
        isMagneticMode = true;
        magneticTimer = duration;
        Debug.Log($"Magnetic mode activated for {duration} seconds!");
    }

    // Method to trigger permanent magnetic mode for debugging
    public void TriggerPermanentMagneticMode()
    {
        isMagneticMode = true;
        magneticTimer = float.MaxValue; // Set to maximum value for "permanent"
        Debug.Log("Permanent magnetic mode activated for debugging!");
    }

    private void HandleMagneticMode()
    {
        if (isMagneticMode)
        {
            magneticTimer -= Time.fixedDeltaTime;
            if (magneticTimer <= 0)
            {
                isMagneticMode = false;
                Debug.Log("Magnetic mode ended");
                return;
            }

            // Find all nearby coins and attract them
            AttractNearbyCoins();
        }
    }

    private void AttractNearbyCoins()
    {
        // Find all GameObjects with "Coin" tag within magnetic range
        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        
        foreach (GameObject coin in coins)
        {
            float distance = Vector2.Distance(transform.position, coin.transform.position);
            
            // Only attract coins within range
            if (distance <= magneticRange)
            {
                // Calculate direction from coin to player
                Vector2 direction = (transform.position - coin.transform.position).normalized;
                
                // Get or add Rigidbody2D to the coin for physics-based attraction
                Rigidbody2D coinRb = coin.GetComponent<Rigidbody2D>();
                if (coinRb != null)
                {
                    // Apply force towards player, stronger when closer
                    float forceMultiplier = (magneticRange - distance) / magneticRange; // Stronger when closer
                    coinRb.AddForce(direction * magneticForce * forceMultiplier * Time.fixedDeltaTime);
                }
                else
                {
                    // If no Rigidbody2D, move directly (simpler approach)
                    float moveSpeed = magneticForce * Time.fixedDeltaTime * 0.01f;
                    coin.transform.position = Vector2.MoveTowards(coin.transform.position, transform.position, moveSpeed);
                }
            }
        }
    }

    // Used by rock controller
    public void RampingRockTrickHandler()
    {
        GameManager.Instance.IncreaseScoreFromPlayerTrick(GameConstants.RockSmash);
    }

    private void HandleBoostPlayer()
    {
        if (isBoosting)
        {
            boostTimer -= Time.fixedDeltaTime;
            if (boostTimer <= 0)
            {
                isBoosting = false;
                boostTimer = boostDuration;
                speedBoostParticlePrefab.Stop();
                
                // Reset speed to default when boost ends
                terrainManager.SetSurfaceSpeed(terrainManager.baseSpeed);
                Debug.Log("Boost ended, speed reset to default");
                return;
            }

            // Boosting stage - apply speed boost
            float t = 1f - (boostTimer / boostDuration);

            // Explanation:
            // 1f is the base speed
            // Ex: base speed: 1f, boostMultiplier: 2f => The additional speed is 1f
            // So we want to manipulate the additional speed, not the boost multiplier itself
            float multiplier = 1f + boostCurve.Evaluate(t) * (boostMultiplier - 1f);

            terrainManager.SetSurfaceSpeed(terrainManager.baseSpeed * multiplier);
        }
    }

    private void DecreaseJumpBufferTime()
    {
        if (bufferJump)
        {
            bufferRemainingTime -= Time.deltaTime;
            if (bufferRemainingTime <= 0f)
            {
                bufferJump = false; // Reset the buffer if time runs out
                bufferRemainingTime = 0.2f; // Reset the buffer time for next use
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Ground"))
        {
            isGrounded = true;
            // Check if we have a buffered jump
            if (bufferJump)
            {
                Debug.Log("Buffered jump executed");
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                bufferJump = false; // Reset the buffer after executing the jump
            }

            // Play the drifting particle effect that follow this player
            driftingParticlePrefab.Play();

            // Reset the rotation counters
            RotatedDegree = 0f;
            accumulated360Rotations = 0f;

            // Trigger boost if player performed tricks while airborne
            if ((didFlip || did360) && !IsPlayerLost)
            {
                TriggerBoostMode(boostDuration);
                didFlip = false;
                did360 = false;
            }

            surfaceNormal = other.contacts[0].normal;
            cameraZoomEffect.ResetZoom();
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.collider.CompareTag("Ground"))
        {
            isGrounded = false;
            driftingParticlePrefab.Stop();
            surfaceNormal = Vector2.up; // Reset to default
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            Debug.Log("Ouch, my head");
            
            // Stop boost mode immediately if player hits head while boosting
            if (isBoosting)
            {
                isBoosting = false;
                speedBoostParticlePrefab.Stop();
                terrainManager.SetSurfaceSpeed(terrainManager.baseSpeed);
                Debug.Log("Boost mode stopped due to head collision!");
            }
            
            PlayerGameOver();
        }
        // else if (other.CompareTag("PowerUp"))
        // {
        //     Debug.Log("Power-up collected! Boost mode activated for 8 seconds.");
        //     TriggerBoostMode(8f); // 8 second boost duration

        //     // Destroy or disable the power-up after collection with null check
        //     if (other != null && other.gameObject != null)
        //     {
        //         Destroy(other.gameObject);
        //     }
        // }
        // else if (other.CompareTag("Magnetic"))
        // {
        //     Debug.Log("Magnetic power-up collected! Magnetic mode activated for 8 seconds.");
        //     TriggerMagneticMode(8f); // 8 second magnetic mode duration

        //     // Destroy or disable the power-up after collection with null check
        //     if (other != null && other.gameObject != null)
        //     {
        //         Destroy(other.gameObject);
        //     }
        // }
    }

    public void PlayerGameOver()
    {
        IsPlayerLost = true;
        //rb.simulated = false;
        terrainManager.SetSurfaceSpeed(0);
        speedBoostParticlePrefab.Stop();
        Instantiate(deadParticlePrefab, transform.position, Quaternion.identity);
        audioSource.PlayOneShot(crashSound);
        animator.SetBool("IsDead", true);
        //GameManager.Instance.PlayerLost();
    }

    public void Jump()
    {
        if ((isGrounded))
        {
            Vector2 jumpVector = Vector2.up + Vector2.right * 0.5f; // Small forward push
            rb.AddForce(jumpVector * jumpForce, ForceMode2D.Impulse);
            cameraZoomEffect?.ZoomOut();
        }
        else
        {
            bufferJump = true;
        }
    }

    public void ToggleMainMenu()
    {
        GameManager.Instance.PauseGame();
    }
}