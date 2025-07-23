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
    public void ShowHUD() => SwitchMenu(hudGame);

    public void OpenSettings(bool fromPause)
    {
        fromPauseMenu = fromPause;
        SwitchMenu(settingsMenu);
        if (hudGame != null)
            hudGame.SetActive(false); // Hide HUD when opening settings
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
        if (startMenu != null) startMenu.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (settingsMenu != null) settingsMenu.SetActive(false);
        if (shopMenu != null) shopMenu.SetActive(false);
        if (gameOverMenu != null) gameOverMenu.SetActive(false);
        if (hudGame != null) hudGame.SetActive(false); // Hide HUD by default

        if (menuToShow != null) menuToShow.SetActive(true);
    }
}
