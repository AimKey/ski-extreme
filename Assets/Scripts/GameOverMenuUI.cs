<<<<<<< HEAD
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    [Header("HUD Text")]
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text yourScoreText;
    [SerializeField] private TMP_Text coinText;

    [Header("Buttons")]
    [SerializeField] private GameObject homeButton;
    [SerializeField] private GameObject playAgainButton;

    private void OnEnable()
    {
        // Get highest score from SaveManager
        int highScore = SaveManager.Instance.LoadHighScore();
        // Get your score from ScoreManager
        int yourScore = ScoreManager.Instance.GetTotalScore();
        // Get coins from CoinManager
        int coins = CoinManager.Instance.GetTotalCoins();

        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;

        if (yourScoreText != null)
            yourScoreText.text = "Your Score: " + yourScore;

        if (coinText != null)
            coinText.text = "Coins: " + coins;
    }

    public void OnHomeButton()
    {
        SceneManager.LoadScene("UIMenu"); 
    }

    public void OnPlayAgainButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
=======
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    [Header("HUD Text")]
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text yourScoreText;
    [SerializeField] private TMP_Text coinText;

    [Header("Buttons")]
    [SerializeField] private GameObject homeButton;
    [SerializeField] private GameObject playAgainButton;

    private void OnEnable()
    {
        // Get highest score from SaveManager
        int highScore = SaveManager.Instance.LoadHighScore();
        // Get your score from ScoreManager
        int yourScore = ScoreManager.Instance.GetTotalScore();
        // Get coins from CoinManager
        int coins = CoinManager.Instance.GetTotalCoins();

        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;

        if (yourScoreText != null)
            yourScoreText.text = "Your Score: " + yourScore;

        if (coinText != null)
            coinText.text = "Coins: " + coins;
    }

    public void OnHomeButton()
    {
        SceneManager.LoadScene("UIMenu"); 
    }

    public void OnPlayAgainButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
>>>>>>> 399cd215232780a03106132c7dc5edecb46143ed
