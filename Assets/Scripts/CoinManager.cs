using TMPro;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    [SerializeField] private TMP_Text coinText;

    private int totalCoins = 0;


    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Load từ SaveManager thay vì PlayerPrefs trực tiếp
        //totalCoins = SaveManager.Instance.LoadTotalCoins();
        totalCoins = 0;
        UpdateUI();
    }

    public void AddCoin(int amount)
    {
        totalCoins += amount;
        SaveCoins();
        UpdateUI();
    }

    public void SpendCoins(int amount)
    {
        if (totalCoins >= amount)
        {
            totalCoins -= amount;
            SaveCoins();
            UpdateUI();
        }
        else
        {
            Debug.Log("Not enough coins");
        }
    }

    public int GetTotalCoins()
    {
        return totalCoins;
    }

    private void SaveCoins()
    {
        SaveManager.Instance.SaveTotalCoins(totalCoins);
    }

    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = "Coins: " + totalCoins;
    }
}
