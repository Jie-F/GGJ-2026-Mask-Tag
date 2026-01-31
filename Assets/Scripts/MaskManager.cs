using System.Diagnostics;
using UnityEngine;

public class MaskManager : MonoBehaviour
{
    public static MaskManager Instance;

    public MaskOwner currentOwner = MaskOwner.Player;

    public float maskDuration = 30f;
    private float currentTimer;

    private bool canTransfer = true;
    private float transferCooldown = 1f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentTimer = maskDuration;
    }

    void Update()
    {
        currentTimer -= Time.deltaTime;

        if (currentTimer <= 0f)
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
        if (!canTransfer) return;

        // Switch owner
        currentOwner = (currentOwner == MaskOwner.Player)
            ? MaskOwner.Enemy
            : MaskOwner.Player;

        // Reset timer
        currentTimer = maskDuration;

        // Start cooldown
        StartCoroutine(TransferCooldown());
    }

    private System.Collections.IEnumerator TransferCooldown()
    {
        canTransfer = false;
        yield return new WaitForSeconds(transferCooldown);
        canTransfer = true;
    }

    void EndGame()
    {
        if (currentOwner == MaskOwner.Player)
        {
            UnityEngine.Debug.Log("PLAYER DIES");
        }
        else
        {
            UnityEngine.Debug.Log("ENEMY DIES");
        }

        Time.timeScale = 0f; // Simple game stop
    }

    public float GetTimerNormalized()
    {
        return currentTimer / maskDuration;
    }
}
