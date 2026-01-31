using UnityEngine;

public class PlayerMaskInteraction : MonoBehaviour
{
    public float interactDistance = 4f;
    public LayerMask enemyLayer;

    Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    public void TryTransferMask()
    {
        if (MaskManager.Instance.currentOwner != MaskOwner.Player)
            return;

        if (!MaskManager.Instance.CanTransfer())
            return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, enemyLayer))
        {
            EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                MaskManager.Instance.TransferMask();
            }
        }
    }
}
