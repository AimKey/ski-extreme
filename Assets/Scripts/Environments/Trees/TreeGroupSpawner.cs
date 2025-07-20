using UnityEngine;

public class TreeGroupSpawner : MonoBehaviour
{

    public GameObject[] cloudPrefabs;
    public int cloudCount = 10;

    public Vector2 xRange = new Vector2(-10f, 10f);
    public Vector2 speedRange = new Vector2(0.1f, 0.6f);

    private float minY;
    private float maxY;

    void Start()
    {
        CalculateYBoundsFromCamera();

        for (int i = 0; i < cloudCount; i++)
        {
            GameObject prefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Length)];

            float x = Random.Range(xRange.x, xRange.y);
            float y = Random.Range(minY, maxY);
            Vector3 spawnPos = new Vector3(x, y, 0f);

            GameObject cloud = Instantiate(prefab, spawnPos, Quaternion.identity, transform);

            CloudMove moveScript = cloud.GetComponent<CloudMove>();
            if (moveScript != null)
            {
                moveScript.minSpeed = speedRange.x;
                moveScript.maxSpeed = speedRange.y;
                moveScript.moveSpeed = Random.Range(speedRange.x, speedRange.y);
            }
        }
    }

    void CalculateYBoundsFromCamera()
    {
        Camera cam = Camera.main;
        float camHeight = 2f * cam.orthographicSize;
        float camBottom = cam.transform.position.y - camHeight / 2;
        float camTop = cam.transform.position.y + camHeight / 2;

        minY = camBottom + 0.5f;
        maxY = camTop - 0.5f;
    }

}
