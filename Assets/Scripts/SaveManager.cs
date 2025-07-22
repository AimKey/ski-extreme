<<<<<<< HEAD
﻿using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private const string COIN_KEY = "TotalCoins";
    private const string HIGH_SCORE_KEY = "HighScore";
    private const string IMMORTAL_LEVEL_KEY = "ImmortalLevel";
    private const string MAGNET_LEVEL_KEY = "MagnetLevel";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void SaveTotalCoins(int totalCoins)
    {
        PlayerPrefs.SetInt(COIN_KEY, totalCoins);
    }

    public int LoadTotalCoins()
    {
        return PlayerPrefs.GetInt(COIN_KEY, 0);
    }

    public void SaveHighScore(int score)
    {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, score);
    }

    public int LoadHighScore()
    {
        return PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    public void SaveImmortalLevel(int level)
    {
        PlayerPrefs.SetInt(IMMORTAL_LEVEL_KEY, level);
    }

    public int LoadImmortalLevel()
    {
        return PlayerPrefs.GetInt(IMMORTAL_LEVEL_KEY, 0);
    }

    public void SaveMagnetLevel(int level)
    {
        PlayerPrefs.SetInt(MAGNET_LEVEL_KEY, level);
    }

    public int LoadMagnetLevel()
    {
        return PlayerPrefs.GetInt(MAGNET_LEVEL_KEY, 0);
    }
}
=======
﻿using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private const string COIN_KEY = "TotalCoins";
    private const string HIGH_SCORE_KEY = "HighScore";
    private const string BOARD_SPEED_KEY = "BoardSpeedUpgrade";
    private const string CHARACTER_SPIN_KEY = "CharacterSpinUpgrade";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ==== Coins ====
    public void SaveTotalCoins(int totalCoins)
    {
        PlayerPrefs.SetInt(COIN_KEY, totalCoins);
        PlayerPrefs.Save();
    }

    public int LoadTotalCoins()
    {
        return PlayerPrefs.GetInt(COIN_KEY, 0);
    }

    // ==== High Score ====
    public void SaveHighScore(int score)
    {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, score);
        PlayerPrefs.Save();
    }

    public int LoadHighScore()
    {
        return PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    // ==== Upgrades ====
    public void SaveBoardSpeedUpgrade(int level)
    {
        PlayerPrefs.SetInt(BOARD_SPEED_KEY, level);
        PlayerPrefs.Save();
    }

    public int LoadBoardSpeedUpgrade()
    {
        return PlayerPrefs.GetInt(BOARD_SPEED_KEY, 0); // default level 0
    }

    public void SaveCharacterSpinUpgrade(int level)
    {
        PlayerPrefs.SetInt(CHARACTER_SPIN_KEY, level);
        PlayerPrefs.Save();
    }

    public int LoadCharacterSpinUpgrade()
    {
        return PlayerPrefs.GetInt(CHARACTER_SPIN_KEY, 0);
    }
}
>>>>>>> 399cd215232780a03106132c7dc5edecb46143ed
