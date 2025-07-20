using UnityEngine;

public class AlwaysRaycasting : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Create a downward raycast from the object's position
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity);
        // Draw a yellow line in the Scene view to visualize the raycast
        Debug.DrawRay(transform.position, Vector2.down * 100f, Color.yellow);
        if (hit.collider != null)
        {
            // If the raycast hits something, log the hit point
            Debug.Log($"Raycast hit object: {hit.collider.name} at position: {hit.point}");
        }
        else
        {
            // If the raycast does not hit anything, log that it missed
            Debug.Log("Raycast did not hit anything.");
        }
    }
}
