using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Menus")]
    public GameObject mainMenu;
    public GameObject settingsMenu;

    void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        UnityEngine.Debug.Log("ShowMainMenu Called");
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
    }

    public void ShowSettingsMenu()
    {
        UnityEngine.Debug.Log("ShowSettingsMenu Called");
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
