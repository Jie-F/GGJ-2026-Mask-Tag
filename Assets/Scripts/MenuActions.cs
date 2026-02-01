using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuActions : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log("Trying to play the game from the MenuActions.cs");
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
