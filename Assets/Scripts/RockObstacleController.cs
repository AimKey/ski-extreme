using System;
using Unity.Cinemachine;
using UnityEngine;

public class RockObstacleController : MonoBehaviour
{
    // Ref to the child colliders
    [SerializeField] private Collider2D bounceCollider;
    [SerializeField] private Collider2D destroyCollider;
	[SerializeField] private GameObject rockDestroyedVFX;
    [SerializeField] private AudioSource breakRockAudioSource;
    [SerializeField] private CinemachineImpulseSource cinemachineImpulseSource;
    
    private bool isDestroyed = false;
    
    private void Start()
    {
        breakRockAudioSource = GetComponent<AudioSource>();
    }

    // Use to allow player to ramp through the rock
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"RockObstacleController: OnTriggerEnter2D called with {other.name} at position {other.transform.position}");
        if (other.CompareTag("Player"))
        {
            // Prevent multiple triggers from destroying the rock
            if (isDestroyed) return;
            
            PlayerController playerController = PlayerController.Instance;
            
            // Debug information
            bool touchingDestroy = other.IsTouching(destroyCollider);
            bool touchingBounce = other.IsTouching(bounceCollider);
            bool isBoosting = playerController.isBoosting;
            bool isBoxCollider = other is BoxCollider2D;
            
            Debug.Log($"Rock collision debug - Boosting: {isBoosting}, TouchingDestroy: {touchingDestroy}, TouchingBounce: {touchingBounce}, IsBoxCollider: {isBoxCollider}");
            
            // PRIORITY 1: If player is boosting, check for destruction first (regardless of collider type)
            if (isBoosting && touchingDestroy)
            {
                isDestroyed = true; // Mark as destroyed
                PlayerController.Instance.RampingRockTrickHandler();
                ShakeScreen();
                
                // Play break sound if AudioSource exists
                if (breakRockAudioSource != null)
                {
                    breakRockAudioSource.Play();
                    Debug.Log("Player is boosting and hit the destroy area, destroying the rock.");
                    
                    // Spawn VFX if available
                    if (rockDestroyedVFX != null)
                    {
                        Instantiate(rockDestroyedVFX, transform.position, Quaternion.identity);
                    }
                    else
                    {
                        Debug.LogWarning("RockDestroyedVFX not assigned, skipping visual effect.");
                    }
                    
                    // Turn off the rock sprite renderer to hide the rock
                    GetComponent<SpriteRenderer>().enabled = false;
                    Destroy(gameObject, breakRockAudioSource.clip.length);
                }
                else
                {
                    Debug.LogWarning("AudioSource not found on rock, destroying without sound.");
                    Debug.Log("Player is boosting and hit the destroy area, destroying the rock.");
                    
                    // Spawn VFX if available
                    if (rockDestroyedVFX != null)
                    {
                        Instantiate(rockDestroyedVFX, transform.position, Quaternion.identity);
                    }
                    else
                    {
                        Debug.LogWarning("RockDestroyedVFX not assigned, skipping visual effect.");
                    }
                    
                    // Turn off the rock sprite renderer to hide the rock
                    GetComponent<SpriteRenderer>().enabled = false;
                    Destroy(gameObject, 1f); // Use default 1 second delay
                }
                return; // Exit early after destruction
            }
            
            // PRIORITY 2: If not boosting and hitting destroy area, game over
            if (touchingDestroy)
            {
                isDestroyed = true; // Mark as destroyed for this path
                Debug.Log("Player is touching the destroy collider without boost, triggering game over.");
                playerController.PlayerGameOver();
                return; // Exit early after game over
            }
            
            // PRIORITY 3: If hitting bounce area (and not destroy area), bounce
            if (isBoxCollider && touchingBounce && !touchingDestroy)
            {
                Debug.Log("Player's body hit the rock bounce collider - bouncing!");
                playerController.BounceOffRock(transform.position);
                return; // Don't destroy or game over, just bounce
            }
        }
    }

    private void ShakeScreen()
    {
        cinemachineImpulseSource.GenerateImpulse();
    }
}