using UnityEngine;

public class PowerupController : MonoBehaviour
{
    public GameObject powerupPrefab; // Reference to the powerup prefab
    [Tooltip("Either magnet or shield")]
    public string PowerupName;
    private PlayerController player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        player = PlayerController.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    // Check if the collided object is a player
    //    if (collision.CompareTag("Player"))
    //    {
    //        // Swtich either magnet or shield
    //        if (powerupPrefab != null)
    //        {
    //            if (PowerupName.ToLower() == "magnet")
    //            {
    //                player.TriggerMagneticMode(10f);
    //            }
    //            else if (PowerupName.ToLower() == "shield")
    //            {
    //                player.
    //            }
    //        }
    //    }
    //}
}
