using UnityEngine;

public class CloudMove : MonoBehaviour
{

    [Header("Speed")]
    public float minSpeed = 0.1f;
    public float maxSpeed = 0.6f;

    [HideInInspector] public float moveSpeed;

    private float width;
    private UnityEngine.Camera mainCam;
    private float screenLeft, screenRight;

    void Start()
    {
        mainCam = UnityEngine.Camera.main;
        width = GetComponent<SpriteRenderer>().bounds.size.x;
        moveSpeed = Random.Range(minSpeed, maxSpeed);
    }

    void Update()
    {
        UpdateScreenBounds();

        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);

        if (transform.position.x + width < screenLeft)
        {
            float newX = screenRight + width;
            float camHeight = 2f * mainCam.orthographicSize;
            float camMidY = mainCam.transform.position.y;
            float camTop = camMidY + camHeight / 2f;

            // Recalculate Y within the upper half of the screen
            float newY = Random.Range(camMidY, camTop);

            transform.position = new Vector3(newX, newY, transform.position.z);
            moveSpeed = Random.Range(minSpeed, maxSpeed);
        }

    }

    void UpdateScreenBounds()
    {
        float height = 2f * mainCam.orthographicSize;
        float width = height * mainCam.aspect;

        screenLeft = mainCam.transform.position.x - width / 2;
        screenRight = mainCam.transform.position.x + width / 2;
    }

}
