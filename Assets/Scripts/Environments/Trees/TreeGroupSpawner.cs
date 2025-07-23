using UnityEngine;

public class TreeGroupSpawner : MonoBehaviour
{

    [Header("Tree Prefabs")]
    public GameObject[] treePrefabs;

    [Header("Spawn Settings")]
    public int minTreeCount = 3;
    public int maxTreeCount = 5;
    public float spacing = 0.6f;
    public Vector2 randomYOffset = new Vector2(-0.1f, 0.1f); // Tùy chọn lệch trục Y

    [Header("Scale Settings")]
    public float minScale = 0.9f;
    public float maxScale = 1.3f;

    void Start()
    {
        SpawnTrees();
    }

    void SpawnTrees()
    {
        if (treePrefabs == null || treePrefabs.Length == 0)
        {
            Debug.LogWarning("No tree prefabs assigned.");
            return;
        }

        int count = Random.Range(minTreeCount, maxTreeCount + 1);
        float startX = -(count - 1) * spacing / 2f;

        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3(startX + i * spacing, Random.Range(randomYOffset.x, randomYOffset.y), 0f);
            GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
            GameObject tree = Instantiate(prefab, transform.position + offset, Quaternion.identity, transform);

            float scale = Random.Range(minScale, maxScale);
            tree.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

}
