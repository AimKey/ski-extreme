using UnityEngine;

/// <summary>
/// Rewards players with speed boosts for performing aerial flips.
/// - Tracks rotation accumulation during airborne phases
/// - Triggers speed boost when landing after completing ~360° rotation
/// - Provides tolerance for imperfect flips
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(SlideController))]
[DefaultExecutionOrder(0)]
public class FlipReward : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Flip Detection")]
    [SerializeField] private float flipTolerance = 20f;        // How close to 360° counts as a flip
    [SerializeField] private float minFlipAngle = 340f;        // Minimum rotation to count as flip (360 - tolerance)
    
    #endregion

    #region Private Fields
    
    // Components
    private Rigidbody2D rb;
    private SlideController slide;
    
    // Rotation tracking
    private float spinAccumulation;
    private float lastRotation;
    
    // State tracking
    private bool wasGroundedLastFrame = true;
    
    #endregion

    #region Public Properties
    
    /// <summary>Current accumulated rotation in degrees</summary>
    public float CurrentSpinAccumulation => spinAccumulation;
    
    /// <summary>Whether player is currently building up a flip</summary>
    public bool IsBuiltingFlip => !slide.IsGrounded && spinAccumulation > 0f;
    
    #endregion

    #region Unity Lifecycle
    
    void Awake()
    {
        InitializeComponents();
        InitializeValues();
    }

    void FixedUpdate()
    {
        UpdateRotationTracking();
        CheckForLanding();
        UpdatePreviousFrameState();
    }
    
    #endregion

    #region Initialization
    
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        slide = GetComponent<SlideController>();
    }
    
    private void InitializeValues()
    {
        lastRotation = rb.rotation;
        minFlipAngle = 360f - flipTolerance;
    }
    
    #endregion

    #region Rotation Tracking
    
    private void UpdateRotationTracking()
    {
        if (slide.IsGrounded) return;
        
        AccumulateRotation();
    }
    
    private void AccumulateRotation()
    {
        float currentRotation = rb.rotation;
        float rotationDelta = Mathf.Abs(Mathf.DeltaAngle(currentRotation, lastRotation));
        
        spinAccumulation += rotationDelta;
        lastRotation = currentRotation;
    }
    
    #endregion

    #region Landing Detection & Rewards
    
    private void CheckForLanding()
    {
        bool isGroundedNow = slide.IsGrounded;
        bool justLanded = isGroundedNow && !wasGroundedLastFrame;
        
        if (justLanded)
        {
            ProcessLanding();
        }
    }
    
    private void ProcessLanding()
    {
        if (IsSuccessfulFlip())
        {
            RewardPlayer();
        }
        
        ResetRotationTracking();
    }
    
    private bool IsSuccessfulFlip()
    {
        return spinAccumulation >= minFlipAngle;
    }
    
    private void RewardPlayer()
    {
        slide.TriggerSpeedBoost();
        Debug.Log($"Flip completed! Rotation: {spinAccumulation:F1}°");
    }
    
    private void ResetRotationTracking()
    {
        spinAccumulation = 0f;
    }
    
    #endregion

    #region State Management
    
    private void UpdatePreviousFrameState()
    {
        lastRotation = rb.rotation;
        wasGroundedLastFrame = slide.IsGrounded;
    }
    
    #endregion

    #region Debug & Utilities
    
    #if UNITY_EDITOR
    private void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 200, 100));
        GUILayout.Label($"Spin: {spinAccumulation:F1}°");
        GUILayout.Label($"Grounded: {slide.IsGrounded}");
        GUILayout.Label($"Building Flip: {IsBuiltingFlip}");
        GUILayout.EndArea();
    }
    #endif
    
    #endregion
}
