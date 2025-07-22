using System.Collections;
using UnityEngine;

public class PlayerMagnet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float magnetRadius   = 6f;   // scan range
    [SerializeField] float pullSpeed      = 15f;  // m/s toward player
    [SerializeField] float pickupRadius   = 0.8f; // auto-collect distance
    [SerializeField] LayerMask coinMask;          // only Coins layer

    bool magnetActive;
    readonly Collider2D[] buffer = new Collider2D[40]; // reuse to avoid GC

    public void ActivateMagnet(float seconds)
    {
        if (magnetActive) StopAllCoroutines();
        magnetActive = true;
        StartCoroutine(MagnetRoutine(seconds));
    }

    IEnumerator MagnetRoutine(float secs)
    {
        float t = 0f;
        while (t < secs)
        {
            AttractCoins();
            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        magnetActive = false;
    }

    void AttractCoins()
    {
        int count = Physics2D.OverlapCircleNonAlloc(transform.position,
                                                    magnetRadius,
                                                    buffer,
                                                    coinMask);
        for (int i = 0; i < count; i++)
        {
            Collider2D c = buffer[i];
            if (!c || !c.gameObject.activeInHierarchy) continue;

            Vector2 dir = (transform.position - c.transform.position).normalized;
            c.transform.position += (Vector3)(dir * pullSpeed * Time.fixedDeltaTime);

            if (Vector2.Distance(c.transform.position, transform.position) < pickupRadius)
            {
                // c.GetComponent<Coin>().Collect(); // your existing coin logic
            }
        }
    }
}

