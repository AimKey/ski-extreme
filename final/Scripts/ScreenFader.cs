using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader instance;

    public float fadeDuration = 1f;
    private Image blackImage;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            blackImage = GetComponent<Image>();
        }
        else
        {
            Destroy(gameObject);
        }


    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) {
            FadeIn(() =>
            {
                Debug.Log("Fade In Done!");
            });
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            FadeOut(() =>
            {
                Debug.Log("Fade Out Done!");
            });
        }
    }
    // Fade từ đen sang trong suốt (Sáng dần)
    public void FadeIn(Action onComplete = null)
    {
        StartCoroutine(Fade(1f, 0f, onComplete));
    }

    // Fade từ trong suốt sang đen (Tối dần)
    public void FadeOut(Action onComplete = null)
    {
        StartCoroutine(Fade(0f, 1f, onComplete));
    }

    private IEnumerator Fade(float fromAlpha, float toAlpha, Action onComplete)
    {
        float elapsed = 0f;
        Color color = blackImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / fadeDuration);
            blackImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        blackImage.color = new Color(color.r, color.g, color.b, toAlpha);
        onComplete?.Invoke();
    }
}
