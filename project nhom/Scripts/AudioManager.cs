
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip bg;
    [SerializeField] private AudioClip click;
    [SerializeField] private AudioClip falling;
    [SerializeField] private AudioClip hitRock;
    [SerializeField] private AudioClip pickCoin;
    [SerializeField] private AudioClip skilling;

    public float volumeEffect { get; private set; } = 0.5f;
    public float volumeBackground { get; private set; } = 0.5f;
    public bool isSoundMuted { get; private set; } = false;
    private AudioSource bgSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            bgSource = gameObject.AddComponent<AudioSource>();
            bgSource.loop = true;
            bgSource.volume = volumeBackground;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- Background Music ---
    public void PlayBackgroundMusic()
    {
        if (isSoundMuted || bg == null) return;

        if (!bgSource.isPlaying)
        {
            bgSource.clip = bg;
            bgSource.Play();
        }
    }

    public void PauseBackgroundMusic()
    {
        if (bgSource.isPlaying)
        {
            bgSource.Pause();
        }
    }

    // --- Settings ---
    public void SetMute(bool mute)
    {
        isSoundMuted = mute;
        bgSource.mute = mute;
    }

    public void SetVolumeEffect(float volume)
    {
        volumeEffect = Mathf.Clamp01(volume);
    }

    public void SetVolumeBackground(float volume)
    {
        volumeBackground = Mathf.Clamp01(volume);
        bgSource.volume = volumeBackground;
    }

    // --- Sound Effects ---
    public void PlaySoundEffect(AudioClip clip)
    {
        if (isSoundMuted) return;
        if (clip != null)
        {
            GameObject tempGO = new GameObject("Audio");
            AudioSource tempSource = tempGO.AddComponent<AudioSource>();
            tempSource.volume = volumeEffect;
            tempSource.PlayOneShot(clip);
            Destroy(tempGO, clip.length);
        }
    }

    public void PlaySoundClick() => PlaySoundEffect(click);
    public void PlaySoundFalling() => PlaySoundEffect(falling);
    public void PlaySoundHitRock() => PlaySoundEffect(hitRock);
    public void PlaySoundPickCoin() => PlaySoundEffect(pickCoin);
    public void PlaySoundSkilling() => PlaySoundEffect(skilling);
}
