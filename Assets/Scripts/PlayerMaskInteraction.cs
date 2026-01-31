using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMaskInteraction : MonoBehaviour
{
    public float interactDistance = 4f;
    public LayerMask enemyLayer;

    Camera cam;

    void Start()
    {
        cam = Camera.main;
        UnityEngine.Debug.Log("PlayerMaskInteraction ready");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryTransferMaskToEnemy();
        }
    }

    public void TryTransferMaskToEnemy()
    {
        if (MaskManager.Instance.currentOwner != MaskOwner.Player)
        {
            UnityEngine.Debug.Log("Player tried to transfer mask, but does not have it");
            return;
        }

        if (!MaskManager.Instance.CanTransfer())
        {
            UnityEngine.Debug.Log("Mask transfer on cooldown");
            return;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, enemyLayer))
        {
            UnityEngine.Debug.Log("Player clicked enemy — mask transferred to ENEMY");
            MaskManager.Instance.TransferMask();
        }
        else
        {
            UnityEngine.Debug.Log("Player clicked but did not hit enemy");
        }
    }
}
