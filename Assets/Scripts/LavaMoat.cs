using UnityEngine;
using UnityEngine.SceneManagement;

public class LavaKill : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UnityEngine.Debug.Log("PLAYER FELL INTO LAVA - GAME OVER");
            SceneManager.LoadScene("GameOver");
            MusicManager.Instance.PlayGameOverMusic();
        }
    }
}
