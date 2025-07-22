using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ShopUI : MonoBehaviour
{
    public TMP_Text magnetLevelText;
    public TMP_Text immortalLevelText;
    public TMP_Text totalCoinsText;

    public TMP_Text upgradeMagnetButtonText;
    public TMP_Text upgradeImmortalButtonText;

    private const int maxLevel = 3;
    private const int upgradeCost = 100;

    private void Start()
    {
        UpdateUI();
    }

    public void UpgradeMagnet()
    {
        int level = SaveManager.Instance.LoadMagnetLevel();
        if (level >= maxLevel) return;

        if (CoinManager.Instance.GetTotalCoins() >= upgradeCost)
        {
            CoinManager.Instance.SpendCoins(upgradeCost);
            SaveManager.Instance.SaveMagnetLevel(level + 1);
            UpdateUI();
        }
    }

    public void UpgradeImmortal()
    {
        int level = SaveManager.Instance.LoadImmortalLevel();
        if (level >= maxLevel) return;

        if (CoinManager.Instance.GetTotalCoins() >= upgradeCost)
        {
            CoinManager.Instance.SpendCoins(upgradeCost);
            SaveManager.Instance.SaveImmortalLevel(level + 1);
            UpdateUI();
        }
    }

    public void BackToStartMenu()
    {
        SceneManager.LoadScene("StartMenu");
    }

    private void UpdateUI()
    {
        int magnetLevel = SaveManager.Instance.LoadMagnetLevel();
        int immortalLevel = SaveManager.Instance.LoadImmortalLevel();
        int coins = CoinManager.Instance.GetTotalCoins();

        // Cập nhật Level Text
        magnetLevelText.text = $"Magnet Level: {(magnetLevel >= maxLevel ? "MAX" : magnetLevel.ToString())}";
        immortalLevelText.text = $"Immortal Level: {(immortalLevel >= maxLevel ? "MAX" : immortalLevel.ToString())}";

        // Cập nhật Total Coins
        totalCoinsText.text = $"Total Coins: {coins}";

        // Lấy Button từ Text (dùng GetComponentInParent)
        var magnetButton = upgradeMagnetButtonText.GetComponentInParent<Button>();
        var immortalButton = upgradeImmortalButtonText.GetComponentInParent<Button>();

        // Cập nhật nút Magnet
        if (magnetLevel >= maxLevel)
        {
            upgradeMagnetButtonText.text = "MAXED";
            if (magnetButton != null) magnetButton.interactable = false;
        }
        else
        {
            upgradeMagnetButtonText.text = $"Upgrade ({upgradeCost} coins)";
            if (magnetButton != null) magnetButton.interactable = coins >= upgradeCost;
        }

        // Cập nhật nút Immortal
        if (immortalLevel >= maxLevel)
        {
            upgradeImmortalButtonText.text = "MAXED";
            if (immortalButton != null) immortalButton.interactable = false;
        }
        else
        {
            upgradeImmortalButtonText.text = $"Upgrade ({upgradeCost} coins)";
            if (immortalButton != null) immortalButton.interactable = coins >= upgradeCost;
        }
    }
}
