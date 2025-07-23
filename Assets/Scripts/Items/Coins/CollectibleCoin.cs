using UnityEngine;

public class CollectibleCoin : MonoBehaviour
{
    public int coinValue = 1;
    public AudioClip collectSound;
    public GameObject CoinPickupVFX;
    public AudioSource audioSource;

    private void Start()
    {
        //Destroy(gameObject, lifetime); // Tự động hủy sau lifetime giây
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Hide the sprite
            GetComponent<SpriteRenderer>().enabled = false;

            // Tăng điểm
            CoinManager.Instance.AddCoin(coinValue);

            // Âm thanh (nếu có)
            if (collectSound != null)
                audioSource.PlayOneShot(collectSound);
            // Init the p
            GameObject.Instantiate(CoinPickupVFX, transform.position, Quaternion.identity);

            Destroy(gameObject, 2f);
        }
    }
}
