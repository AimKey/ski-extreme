using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject startMenu;
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public GameObject shopMenu;
    public GameObject gameOverMenu;
    public GameObject hudGame;

    private bool fromPauseMenu = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void ShowStartMenu() => SwitchMenu(startMenu);
    public void ShowPauseMenu() => SwitchMenu(pauseMenu);
    public void ShowGameOverMenu() => SwitchMenu(gameOverMenu);
    public void ShowShopMenu() => SwitchMenu(shopMenu);

    public void OpenSettings(bool fromPause)
    {
        fromPauseMenu = fromPause;
        SwitchMenu(settingsMenu);
        hudGame?.SetActive(false);
    }

    public void BackFromSettings()
    {
        if (fromPauseMenu)
            ShowPauseMenu();
        else
            ShowStartMenu();
    }

    private void SwitchMenu(GameObject menuToShow)
    {
        startMenu?.SetActive(false);
        pauseMenu?.SetActive(false);
        settingsMenu?.SetActive(false);
        shopMenu?.SetActive(false);
        gameOverMenu?.SetActive(false);
        hudGame?.SetActive(false); // Hide HUD by default

        menuToShow?.SetActive(true);
    }
}
