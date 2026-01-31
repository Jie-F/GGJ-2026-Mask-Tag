using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public NavMeshAgent agent;

    [Header("Behavior")]
    public float chaseDistance = 3f;
    public float fleeDistance = 8f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            player = playerObj.transform;
        }

        UnityEngine.Debug.Log("EnemyAI started");
    }

    void Update()
    {
        if (MaskManager.Instance.currentOwner == MaskOwner.Enemy)
        {
            ChasePlayer();
        }
        else
        {
            FleeFromPlayer();
        }
    }

    void ChasePlayer()
    {
        agent.SetDestination(player.position);
        //UnityEngine.Debug.Log("Enemy is CHASING the player");

        TryGiveMaskToPlayer();
    }

    void FleeFromPlayer()
    {
        Vector3 awayDirection = (transform.position - player.position).normalized;
        Vector3 fleeTarget = transform.position + awayDirection * fleeDistance;

        agent.SetDestination(fleeTarget);
        //UnityEngine.Debug.Log("Enemy is FLEEING from the player");
    }
    /*
    void TryGiveMaskToPlayer()
    {
        if (!MaskManager.Instance.CanTransfer())
            return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > chaseDistance)
            return;

        Vector3 direction = (player.position - transform.position).normalized;
        Ray ray = new Ray(transform.position + Vector3.up, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, chaseDistance))
        {
            if (hit.transform == player)
            {
                UnityEngine.Debug.Log("Enemy transferred mask to PLAYER");
                MaskManager.Instance.TransferMask();
            }
        }
    }*/

    void TryGiveMaskToPlayer()
    {
        if (!MaskManager.Instance.CanTransfer())
            return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > chaseDistance)
            return;

        // Optional: check facing direction
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, dirToPlayer);

        if (dot < 0.5f) // not roughly facing player
            return;

        UnityEngine.Debug.Log("Enemy tagged PLAYER — mask transferred");
        MaskManager.Instance.TransferMask();
    }
}
