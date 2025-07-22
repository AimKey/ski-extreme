using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuUI : MonoBehaviour
{
    public void OnClickStart()
    {
        SceneManager.LoadScene("MainLevel");
    }
    public void OnClickSettings()
    {
        UIManager.Instance.OpenSettings(fromPause: false);
    }

    public void OnClickShop()
    {
        UIManager.Instance.ShowShopMenu();
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
}
