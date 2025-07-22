<<<<<<< HEAD
﻿using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("HUD Text")]
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text trickText;

    [Header("Tracking")]
    [SerializeField] private Transform playerTransform;

    private float startX;
    private float distance;
    private int trickScore ;
    private int totalScore;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public float GetDistance() => distance;

    private void Start()
    {
        if (playerTransform == null)
            Debug.LogError("Player Transform chưa được gán!");

        startX = playerTransform.position.x;
        ResetScores();
    }

    private void Update()
    {
        UpdateDistance();
        UpdateTotalScore();
        UpdateUI();
    }

    private void UpdateDistance()
    {
        distance = Mathf.Max(0, playerTransform.position.x - startX);
    }

    private void UpdateTotalScore()
    {
        totalScore = Mathf.FloorToInt(distance) + trickScore;
    }

    private void UpdateUI()
    {
        if (distanceText != null)
            distanceText.text = "Distance: " + Mathf.FloorToInt(distance).ToString();

        if (scoreText != null)
            scoreText.text = "Score: " + totalScore.ToString();
    }

    public void AddTrickScore(int amount, string trickName)
    {
        trickScore += amount;

        if (trickText != null)
            trickText.text = trickName;
    }

    public int GetTotalScore() => totalScore;

    public void ResetScores()
    {
        trickScore = 0;
        distance = 0f;
        totalScore = 0;
        if (trickText != null)
            trickText.text = "";
    }

    /// <summary>
    /// Gọi hàm này khi Game Over để lưu điểm cao nhất
    /// </summary>
    public void SaveIfHighScore()
    {
        int previousHigh = SaveManager.Instance.LoadHighScore();
        if (totalScore > previousHigh)
        {
            SaveManager.Instance.SaveHighScore(totalScore);
        }
    }

}
=======
﻿using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("HUD Text")]
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text trickText;

    [Header("Tracking")]
    [SerializeField] private Transform playerTransform;

    private float startX;
    private float distance;
    private int trickScore ;
    private int totalScore;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public float GetDistance() => distance;

    private void Start()
    {
        if (playerTransform == null)
            Debug.LogError("Player Transform chưa được gán!");

        startX = playerTransform.position.x;
        ResetScores();
    }

    private void Update()
    {
        UpdateDistance();
        UpdateTotalScore();
        UpdateUI();
    }

    private void UpdateDistance()
    {
        distance = Mathf.Max(0, playerTransform.position.x - startX);
    }

    private void UpdateTotalScore()
    {
        totalScore = Mathf.FloorToInt(distance) + trickScore;
    }

    private void UpdateUI()
    {
        if (distanceText != null)
            distanceText.text = "Distance: " + Mathf.FloorToInt(distance).ToString();

        if (scoreText != null)
            scoreText.text = "Score: " + totalScore.ToString();
    }

    public void AddTrickScore(int amount, string trickName)
    {
        trickScore += amount;

        if (trickText != null)
            trickText.text = trickName;
    }

    public int GetTotalScore() => totalScore;

    public void ResetScores()
    {
        trickScore = 0;
        distance = 0f;
        totalScore = 0;
        if (trickText != null)
            trickText.text = "";
    }

    /// <summary>
    /// Gọi hàm này khi Game Over để lưu điểm cao nhất
    /// </summary>
    public void SaveIfHighScore()
    {
        int previousHigh = SaveManager.Instance.LoadHighScore();
        if (totalScore > previousHigh)
        {
            SaveManager.Instance.SaveHighScore(totalScore);
        }
    }

}
>>>>>>> 399cd215232780a03106132c7dc5edecb46143ed
