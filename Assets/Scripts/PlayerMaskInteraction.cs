using System.Diagnostics;
using UnityEngine;

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

        // Ray from center of screen (Input System safe)
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Vector3 rayOrigin = cam.transform.position;
        Vector3 rayDirection = cam.transform.forward;

        UnityEngine.Debug.DrawRay(rayOrigin, rayDirection * interactDistance, Color.red, 0.1f);

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hitlol, interactDistance, enemyLayer))
        {
            UnityEngine.Debug.Log("Ray hit: " + hitlol.collider.name);
        }
        else
        {
            UnityEngine.Debug.Log("Ray did NOT hit anything");
        }

        // Ignore the player collider layer when doing raycast
        int layerMask = enemyLayer.value & ~(1 << LayerMask.NameToLayer("Player"));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, layerMask))
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
