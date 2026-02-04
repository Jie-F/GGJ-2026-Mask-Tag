using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MaskManager : MonoBehaviour
{
    public static MaskManager Instance;

    public MaskOwner currentOwner = MaskOwner.Enemy;  // Initially enemy has the mask
    public GameObject enemy;
    public GameObject player;

    public float maskDuration = 30f;
    float timer;

    bool canTransfer = true;
    float transferCooldown = 3f;

    [Header("Mask Prefab Settings")]
    public GameObject maskPrefab;
    PrefabAnimatorController maskAnimatorController;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        timer = maskDuration;
        UnityEngine.Debug.Log("Game started, and the ENEMY has the mask");

        // Ensure maskPrefab is hidden initially
        if (maskPrefab != null)
        {
            maskPrefab.SetActive(false);
            maskAnimatorController = maskPrefab.GetComponent<PrefabAnimatorController>();
            if (maskAnimatorController == null)
                Debug.LogWarning("PrefabAnimatorController not found on maskPrefab!");
        }
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

        if (currentOwner == MaskOwner.Player)
        {
            currentOwner = MaskOwner.Enemy;
            MusicManager.Instance.PlayFrozenMusic();

            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.StartCoroutine(enemyAI.StunEnemy());
            }

            // Hide mask from player
            if (maskPrefab != null)
                maskPrefab.SetActive(false);
        } else
        {
            currentOwner = MaskOwner.Player;
            MusicManager.Instance.PlayFireMusic();


            // Show mask on player and play animation
            if (maskPrefab != null)
            {
                maskPrefab.SetActive(true); // show it
                if (maskAnimatorController != null)
                    maskAnimatorController.PlayMaskKill(); // start animation
            }

            PlayerMotor motor = player.GetComponent<PlayerMotor>();
            if (motor != null)
                StartCoroutine(motor.StunPlayer());
        }

        timer = maskDuration;

        UnityEngine.Debug.Log("Mask transferred � New owner: " + currentOwner);


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
            MusicManager.Instance.PlayGameOverMusic();
            Time.timeScale = 1f;
        }
        else
        {
            UnityEngine.Debug.Log("ENEMY DIED - YOU WIN");
            SceneManager.LoadScene("GameWon");
            MusicManager.Instance.PlayGameWinMusic();
            Time.timeScale = 1f;
        }

        Time.timeScale = 0f;
    }

    public float GetTimerNormalized()
    {
        return timer / maskDuration;
    }
}
