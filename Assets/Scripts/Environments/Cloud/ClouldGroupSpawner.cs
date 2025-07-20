using UnityEngine;

public class ClouldGroupSpawner : MonoBehaviour
{
    [Header("Cloud Prefabs")]
    public GameObject[] cloudPrefabs;

        [Header("Spawn Settings")]
        public int cloudCount = 10;
        public Vector2 spawnXRangeOffset = new Vector2(-10f, 10f); // tính từ giữa màn hình
        public float verticalSpawnRatio = 0.5f; // spawn từ giữa trở lên (0.5 = nửa trên)

    [Header("Speed Settings")]
    public Vector2 speedRange = new Vector2(0.1f, 0.6f);

    private UnityEngine.Camera cam;

    void Start()
    {
        cam = UnityEngine.Camera.main;
        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float camLeft = cam.transform.position.x - camWidth / 2f;
        float camRight = cam.transform.position.x + camWidth / 2f;
        float camMidY = cam.transform.position.y;
        float camTop = cam.transform.position.y + camHeight / 2f;

        float spawnMinY = Mathf.Lerp(camMidY, camTop, 0f); // bắt đầu từ giữa
        float spawnMaxY = camTop;

        for (int i = 0; i < cloudCount; i++)
        {
            GameObject prefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Length)];

            float x = Random.Range(camLeft + spawnXRangeOffset.x, camRight + spawnXRangeOffset.y);
            float y = Random.Range(spawnMinY, spawnMaxY);

            GameObject cloud = Instantiate(prefab, new Vector3(x, y, 0f), Quaternion.identity, transform);

            CloudMove cm = cloud.GetComponent<CloudMove>();
            if (cm != null)
            {
                cm.minSpeed = speedRange.x;
                cm.maxSpeed = speedRange.y;
            }
        }
    }
}