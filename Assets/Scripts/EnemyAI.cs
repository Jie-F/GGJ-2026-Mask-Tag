using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public NavMeshAgent agent;

    [Header("Behavior")]
    public float chaseDistance = 3f;
    public float fleeDistance = 8f;
    public float enemyRunSpeed = 8f;
    public float enemyChaseSpeed = 8f;

    [Header("Stuck Detection")]
    public float stuckCheckInterval = 0.25f;
    public float stuckTimeThreshold = 3f;
    public float minMovementDistance = 1.0f;

    [Header("Teleport Recovery")]
    public float teleportRadius = 6f;
    public float teleportHeight = 6f;
    public int teleportAttempts = 60;

    private Vector3 lastPosition;
    private float stuckTimer;
    private float checkTimer;

    [Header("NavMesh Reconnection")]
    public float reconnectCheckInterval = 0.25f;
    public float reconnectCooldown = 3f;
    public float reconnectSampleRadius = 3f;

    private float reconnectTimer;
    private float lastReconnectTime;
    private NavMeshPath sharedPath;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        sharedPath = new NavMeshPath();


        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            player = playerObj.transform;
        }

        lastPosition = transform.position;

        // Make sure the agent is on the NavMesh
        if (!agent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                agent.Warp(hit.position); // immediately place agent on NavMesh
            }
            else
            {
                UnityEngine.Debug.LogWarning("No NavMesh found near the agent!");
            }
        }

        UnityEngine.Debug.Log("EnemyAI started");
    }

    void Update()
    {
        // Only do the disconnected navmesh TP if the enemy is chasing the player!
        // If the TP is done when the player is chasing the enemy, then this will TP the enemy close to the player when the player deliberately goes somewhere wacky lol
        if (MaskManager.Instance.currentOwner == MaskOwner.Enemy)
        {
            CheckNavMeshConnectivity();
        }

        if (MaskManager.Instance.currentOwner == MaskOwner.Enemy)
        {
            agent.speed = enemyChaseSpeed;
            ChasePlayer();
        }
        else
        {
            agent.speed = enemyRunSpeed;
            FleeFromPlayer();
        }

        CheckIfStuck();
    }

    void CheckNavMeshConnectivity()
    {
        reconnectTimer += Time.deltaTime;
        if (reconnectTimer < reconnectCheckInterval)
            return;

        reconnectTimer = 0f;

        if (Time.time - lastReconnectTime < reconnectCooldown)
            return;

        // Sample both positions onto NavMesh
        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit enemyHit, 2f, NavMesh.AllAreas))
            return;

        if (!NavMesh.SamplePosition(player.position, out NavMeshHit playerHit, 2f, NavMesh.AllAreas))
            return;

        // Try to calculate a path
        bool hasPath = NavMesh.CalculatePath(
            enemyHit.position,
            playerHit.position,
            NavMesh.AllAreas,
            sharedPath
        );

        if (!hasPath || sharedPath.status != NavMeshPathStatus.PathComplete)
        {
            UnityEngine.Debug.Log("Enemy on different NavMesh component — reconnecting");
            TeleportToPlayersNavMesh(playerHit.position);
            lastReconnectTime = Time.time;
        }
    }

    void TeleportToPlayersNavMesh(Vector3 playerNavPos)
    {
        // Try closest point first
        if (NavMesh.SamplePosition(
            playerNavPos,
            out NavMeshHit hit,
            reconnectSampleRadius,
            NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            UnityEngine.Debug.Log("Enemy warped onto player's NavMesh component");
            SFXManager.Instance.PlayTeleport();
            return;
        }

        UnityEngine.Debug.LogWarning("Failed to warp enemy to player's NavMesh component");
    }


    void ChasePlayer()
    {
        agent.SetDestination(player.position);
        TryGiveMaskToPlayer();
    }

    void FleeFromPlayer()
    {
        Vector3 awayDirection = (transform.position - player.position).normalized;
        Vector3 fleeTarget = transform.position + awayDirection * fleeDistance;

        agent.SetDestination(fleeTarget);
    }

    void TryGiveMaskToPlayer()
    {
        if (!MaskManager.Instance.CanTransfer())
            return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > chaseDistance)
            return;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, dirToPlayer);

        if (dot < 0.5f)
            return;

        UnityEngine.Debug.Log("Enemy tagged PLAYER — mask transferred");
        MaskManager.Instance.TransferMask();
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

    // ============================
    // STUCK DETECTION & RECOVERY
    // ============================

    void CheckIfStuck()
    {
        checkTimer += Time.deltaTime;
        if (checkTimer < stuckCheckInterval)
            return;

        checkTimer = 0f;

        float distanceMoved = Vector3.Distance(transform.position, lastPosition);

        if (distanceMoved < minMovementDistance && agent.hasPath)
        {
            stuckTimer += stuckCheckInterval;

            if (stuckTimer >= stuckTimeThreshold)
            {
                UnityEngine.Debug.LogWarning("Enemy stuck — attempting teleport recovery");
                TryTeleportToNavMesh();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = transform.position;
    }

    void TryTeleportToNavMesh()
    {
        for (int i = 0; i < teleportAttempts; i++)
        {
            Vector3 randomHorizontal = UnityEngine.Random.insideUnitSphere * teleportRadius;
            randomHorizontal.y = 0f;

            Vector3 sampleOrigin = transform.position + randomHorizontal + Vector3.up * teleportHeight;

            if (NavMesh.SamplePosition(sampleOrigin, out NavMeshHit hit, teleportHeight * 2f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                UnityEngine.Debug.Log("Enemy teleported to recover from stuck state");
                SFXManager.Instance.PlayTeleport();
                return;
            }
        }

        UnityEngine.Debug.LogError("Enemy teleport recovery failed — no valid NavMesh point found");
    }
}
