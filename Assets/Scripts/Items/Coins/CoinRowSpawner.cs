using UnityEngine;

public class CoinRowSpawner : MonoBehaviour
{
    public GameObject[] coinPrefabs;

    public bool randomize = true;

    [Header("Số lượng coin")]
    public int coinCount = 5;
    public int minCoins = 3;
    public int maxCoins = 10;

    [Header("Hình dạng")]
    public bool isRainbow = false;
    public float rainbowAmplitude = 2.5f;

    [Header("Scale & Spacing")]
    public float maxRowWidth = 14f;
    public float coinScale = 0.5f;

    [Header("Spacing Multiplier (0.3 - 1)")]
    [Range(0.3f, 1f)]
    public float spacingMultiplier = 0.6f;

    void Start()
    {
        int count = randomize ? Random.Range(minCoins, maxCoins + 1) : coinCount;
        bool shape = randomize ? (Random.value > 0.5f) : isRainbow;

        // Chọn loại coin duy nhất
        GameObject prefab = coinPrefabs[Random.Range(0, coinPrefabs.Length)];

        float spacing = (maxRowWidth * spacingMultiplier) / Mathf.Max(1, count - 1);

        for (int i = 0; i < count; i++)
        {
            float x = i * spacing;
            float progress = (float)i / (count - 1);
            float y = shape
                ? -4 * rainbowAmplitude * (progress - 0.5f) * (progress - 0.5f) + rainbowAmplitude
                : 0f;

            Vector3 pos = transform.position + new Vector3(x, y, 0);
            GameObject coin = Instantiate(prefab, pos, Quaternion.identity, transform);
            coin.transform.localScale = Vector3.one * coinScale;
        }
    }
}
