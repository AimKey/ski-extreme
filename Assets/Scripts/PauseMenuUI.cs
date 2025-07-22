<<<<<<< HEAD
﻿using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenuUI : MonoBehaviour
{
    public GameObject pauseMenuUI;

    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI coinText;

    void Start()
    {
        pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenuUI.activeSelf)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
        UpdateScoreText();
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("UiMenu"); // đổi tên tùy vào scene bạn đặt
    }

    public void OpenSettings()
    {
        UIManager.Instance.OpenSettings(true); // bạn đã làm sẵn Settings trước
    }

    void UpdateScoreText()
    {
        if (distanceText != null)
            distanceText.text = $"Distance: {ScoreManager.Instance.GetDistance()} m";

        if (scoreText != null)
            scoreText.text = $"Score: {ScoreManager.Instance.GetTotalScore()}";

        if (coinText != null)
            coinText.text = $"Coins: {CoinManager.Instance.GetTotalCoins()}";
    }
}
=======
﻿using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenuUI : MonoBehaviour
{
    public GameObject pauseMenuUI;

    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI coinText;

    void Start()
    {
        pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenuUI.activeSelf)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
        UpdateScoreText();
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("UiMenu"); // đổi tên tùy vào scene bạn đặt
    }

    public void OpenSettings()
    {
        UIManager.Instance.OpenSettings(true); // bạn đã làm sẵn Settings trước
    }

    void UpdateScoreText()
    {
        if (distanceText != null)
            distanceText.text = $"Distance: {ScoreManager.Instance.GetDistance()} m";

        if (scoreText != null)
            scoreText.text = $"Score: {ScoreManager.Instance.GetTotalScore()}";

        if (coinText != null)
            coinText.text = $"Coins: {CoinManager.Instance.GetTotalCoins()}";
    }
}
>>>>>>> 399cd215232780a03106132c7dc5edecb46143ed
