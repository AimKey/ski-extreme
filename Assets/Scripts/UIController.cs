using System.Net.Mime;
using Assets.Scripts.Models;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject TextPopupPrefab;
    [SerializeField] private AnimationClip TextPopupAnimation;
    [SerializeField] private Transform PopupContainer;
    [SerializeField] private TextMeshProUGUI TextScoreText;
    public static UIController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void DisplayPlayerTrick(Trick trick)
    {
        Debug.Log("Player performed trick: " + trick.TrickName);
        // Update the score text
        GameObject textPopupInstance = Instantiate(TextPopupPrefab, PopupContainer);
        var scoreText = textPopupInstance.GetComponentInChildren<TextMeshProUGUI>();
        scoreText.text = $"{trick.TrickName} +{trick.TrickScore}";
        
        textPopupInstance.SetActive(true);
        
        // Use animation length if available, otherwise use default duration
        float destroyDelay = TextPopupAnimation != null ? TextPopupAnimation.length : 2f;
        Destroy(textPopupInstance, destroyDelay);
    }

    public void UpdatePlayerScore(int scoreToUpdate)
    {
        if (TextScoreText != null)
        {
            TextScoreText.text = $"Score: {scoreToUpdate:00000000}";
        }
        else
        {
            Debug.LogWarning("TextScoreText is not assigned in UIController inspector!");
        }
    }
}