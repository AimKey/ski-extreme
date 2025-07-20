using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider soundSlider;
    [SerializeField] private Slider musicSlider;

    [Header("Buttons")]
    [SerializeField] private GameObject backButton;

    private void Start()
    {
        // Optionally load saved values
        soundSlider.value = PlayerPrefs.GetFloat("SoundVolume", 1f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);

        soundSlider.onValueChanged.AddListener(OnSoundSliderChanged);
        musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
    }

    private void OnSoundSliderChanged(float value)
    {
        PlayerPrefs.SetFloat("SoundVolume", value);
        // Add your sound volume logic here
    }

    private void OnMusicSliderChanged(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        // Add your music volume logic here
    }

    public void OnBackButton()
    {
        UIManager.Instance.BackFromSettings();
    }
}
