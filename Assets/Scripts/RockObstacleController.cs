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
            else isDestroyed = true;
            PlayerController playerController = PlayerController.Instance;
            // If player is boosting, destroy the rock instead of losing the game
            if (playerController.isBoosting && other.IsTouching(destroyCollider))
            {
                PlayerController.Instance.RampingRockTrickHandler();
                ShakeScreen();
                
                // Play break sound if AudioSource exists
                if (breakRockAudioSource != null)
                {
                    breakRockAudioSource.Play();
                    Debug.Log("Player is boosting and hit the rock, destroying the rock.");
                    
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
                    Debug.Log("Player is boosting and hit the rock, destroying the rock.");
                    
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
            }
            else if (other.IsTouching(destroyCollider))
            {
                Debug.Log("Player is touching the destroy collider, triggering game over.");
                playerController.PlayerGameOver();
            }
        }
    }

    private void ShakeScreen()
    {
        cinemachineImpulseSource.GenerateImpulse();
    }
}