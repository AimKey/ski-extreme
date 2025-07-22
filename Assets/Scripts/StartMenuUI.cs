<<<<<<< HEAD
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuUI : MonoBehaviour
{
    public void OnClickStart()
    {
        SceneManager.LoadScene("GameScene");
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
=======
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuUI : MonoBehaviour
{
    public void OnClickStart()
    {
        SceneManager.LoadScene("GameScene");
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
>>>>>>> 399cd215232780a03106132c7dc5edecb46143ed
