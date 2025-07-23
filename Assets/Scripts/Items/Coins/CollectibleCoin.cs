using UnityEngine;

public class CollectibleCoin : MonoBehaviour
{
    public int coinValue = 1;
    public AudioClip collectSound;
    //public float lifetime = 10f;

    private void Start()
    {
        //Destroy(gameObject, lifetime); // Tự động hủy sau lifetime giây
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Tăng điểm
            CoinManager.Instance.AddCoin(coinValue);

            // Âm thanh (nếu có)
            if (collectSound != null)
                AudioSource.PlayClipAtPoint(collectSound, transform.position);

            Destroy(gameObject);
        }
    }
}
