using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MaskManager : MonoBehaviour
{
    public static MaskManager Instance;

    public MaskOwner currentOwner = MaskOwner.Enemy;  // Initially enemy has the mask

    public float maskDuration = 30f;
    float timer;

    bool canTransfer = true;
    float transferCooldown = 1f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        timer = maskDuration;
        UnityEngine.Debug.Log("Game started — PLAYER has the mask");
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            EndGame();
        }
    }

    public bool CanTransfer()
    {
        return canTransfer;
    }

    public void TransferMask()
    {
        if (!canTransfer)
            return;

        currentOwner = currentOwner == MaskOwner.Player
            ? MaskOwner.Enemy
            : MaskOwner.Player;

        timer = maskDuration;

        UnityEngine.Debug.Log("Mask transferred — New owner: " + currentOwner);

        StartCoroutine(TransferCooldown());
    }

    IEnumerator TransferCooldown()
    {
        canTransfer = false;
        UnityEngine.Debug.Log("Mask transfer cooldown started");

        yield return new WaitForSeconds(transferCooldown);

        canTransfer = true;
        UnityEngine.Debug.Log("Mask transfer cooldown ended");
    }

    void EndGame()
    {
        if (currentOwner == MaskOwner.Player)
        {
            UnityEngine.Debug.Log("PLAYER DIED - GAME OVER");
            SceneManager.LoadScene("GameOver");
        }
        else
        {
            UnityEngine.Debug.Log("ENEMY DIED - YOU WIN");
            SceneManager.LoadScene("GameWon");
        }

        Time.timeScale = 0f;
    }

    public float GetTimerNormalized()
    {
        return timer / maskDuration;
    }
}
