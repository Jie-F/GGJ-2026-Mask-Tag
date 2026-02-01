using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public NavMeshAgent agent;
    public GameObject mask;

    [Header("Behavior")]
    public float chaseDistance = 3f;
    public float fleeDistance = 10f;
    public float enemyRunSpeed = 8f;
    public float enemyChaseSpeed = 8f;

    [Header("Smart Flee Settings")]
    public int fleeSamples = 16;
    public float fleeSampleRadius = 2.5f;
    public float fleeRepathDistance = 1.5f;
    public float playerPredictionTime = 0.5f;
    public float pathWeight = 0.6f;

    [Header("Stuck Detection")]
    public float stuckCheckInterval = 0.25f;
    public float stuckTimeThreshold = 3f;
    public float minMovementDistance = 1.0f;

    [Header("Teleport Recovery")]
    public float teleportRadius = 6f;
    public float teleportHeight = 6f;
    public int teleportAttempts = 60;

    [Header("NavMesh Reconnection")]
    public float reconnectCheckInterval = 0.25f;
    public float reconnectCooldown = 3f;
    public float reconnectSampleRadius = 3f;

    [Header("Tag Stun")]
    public float tagStunDuration = 1f;

    private Vector3 lastPosition;
    private float stuckTimer;
    private float checkTimer;

    private float reconnectTimer;
    private float lastReconnectTime;
    private NavMeshPath sharedPath;

    private bool isStunned = false;
    private Vector3 currentFleeTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        sharedPath = new NavMeshPath();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        lastPosition = transform.position;

        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
            else
            {
                Debug.LogWarning("Enemy not placed on NavMesh!");
            }
        }
    }

    void Update()
    {
        if (isStunned)
            return;

        if (MaskManager.Instance.currentOwner == MaskOwner.Enemy)
        {
            agent.speed = enemyChaseSpeed;
            mask.SetActive(true);

            CheckNavMeshConnectivity();
            ChasePlayer();
        }
        else
        {
            agent.speed = enemyRunSpeed;
            mask.SetActive(false);

            FleeSmart();
        }

        CheckIfStuck();
    }

    // ============================
    // CHASE
    // ============================

    void ChasePlayer()
    {
        agent.SetDestination(player.position);
        TryGiveMaskToPlayer();
    }

    // ============================
    // SMART FLEE
    // ============================

    void FleeSmart()
    {
        // Recalculate flee path if close to destination or path collapsing
        if (!agent.hasPath || agent.remainingDistance < fleeRepathDistance)
        {
            currentFleeTarget = FindBestFleeTarget();
            agent.SetDestination(currentFleeTarget);
        }
    }

    Vector3 FindBestFleeTarget()
    {
        Vector3 bestTarget = transform.position;
        float bestScore = float.MinValue;

        // Predict player position
        Vector3 predictedPlayerPos = player.position;
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
            predictedPlayerPos += playerRb.linearVelocity * playerPredictionTime;

        for (int i = 0; i < fleeSamples; i++)
        {
            float angle = (360f / fleeSamples) * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
            Vector3 candidate = transform.position + dir * fleeDistance;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, fleeSampleRadius, NavMesh.AllAreas))
                continue;

            NavMeshPath path = new NavMeshPath();
            if (!agent.CalculatePath(hit.position, path))
                continue;

            if (path.status != NavMeshPathStatus.PathComplete)
                continue;

            float playerDist = Vector3.Distance(hit.position, predictedPlayerPos);
            float pathLength = GetPathLength(path);

            // Dead-end avoidance
            if (pathLength < fleeDistance * 0.6f && playerDist < fleeDistance)
                continue;

            float score = playerDist + pathLength * pathWeight;

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = hit.position;
            }
        }

        // Fallback: straight away from player
        if (bestScore == float.MinValue)
        {
            Vector3 fallbackDir = (transform.position - predictedPlayerPos).normalized;
            bestTarget = transform.position + fallbackDir * fleeDistance;
            NavMesh.SamplePosition(bestTarget, out NavMeshHit fallbackHit, fleeSampleRadius, NavMesh.AllAreas);
            bestTarget = fallbackHit.position;
        }

        return bestTarget;
    }

    float GetPathLength(NavMeshPath path)
    {
        float length = 0f;
        for (int i = 1; i < path.corners.Length; i++)
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        return length;
    }

    // ============================
    // MASK TRANSFER
    // ============================

    void TryGiveMaskToPlayer()
    {
        if (!MaskManager.Instance.CanTransfer())
            return;

        if (Vector3.Distance(transform.position, player.position) > chaseDistance)
            return;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        if (Vector3.Dot(transform.forward, dirToPlayer) < 0.5f)
            return;

        MaskManager.Instance.TransferMask();
    }

    // ============================
    // STUN
    // ============================

    public IEnumerator StunEnemy()
    {
        isStunned = true;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        yield return new WaitForSeconds(tagStunDuration);

        agent.isStopped = false;
        isStunned = false;
    }

    // ============================
    // NAVMESH RECONNECTION
    // ============================

    void CheckNavMeshConnectivity()
    {
        reconnectTimer += Time.deltaTime;
        if (reconnectTimer < reconnectCheckInterval)
            return;

        reconnectTimer = 0f;

        if (Time.time - lastReconnectTime < reconnectCooldown)
            return;

        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit enemyHit, 2f, NavMesh.AllAreas))
            return;

        if (!NavMesh.SamplePosition(player.position, out NavMeshHit playerHit, 2f, NavMesh.AllAreas))
            return;

        bool hasPath = NavMesh.CalculatePath(enemyHit.position, playerHit.position, NavMesh.AllAreas, sharedPath);

        if (!hasPath || sharedPath.status != NavMeshPathStatus.PathComplete)
        {
            TeleportToPlayersNavMesh(playerHit.position);
            lastReconnectTime = Time.time;
        }
    }

    void TeleportToPlayersNavMesh(Vector3 playerNavPos)
    {
        if (NavMesh.SamplePosition(playerNavPos, out NavMeshHit hit, reconnectSampleRadius, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);

            SFXManager.Instance.PlayTeleport();
        }
    }

    // ============================
    // STUCK DETECTION
    // ============================

    void CheckIfStuck()
    {
        checkTimer += Time.deltaTime;
        if (checkTimer < stuckCheckInterval)
            return;

        checkTimer = 0f;

        float moved = Vector3.Distance(transform.position, lastPosition);

        if (moved < minMovementDistance && agent.hasPath)
        {
            stuckTimer += stuckCheckInterval;

            if (stuckTimer >= stuckTimeThreshold)
            {
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
            Vector3 offset = UnityEngine.Random.insideUnitSphere * teleportRadius;
            offset.y = 0f;

            Vector3 samplePos = transform.position + offset + Vector3.up * teleportHeight;

            if (NavMesh.SamplePosition(samplePos, out NavMeshHit hit, teleportHeight * 2f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                SFXManager.Instance.PlayTeleport();
                return;
            }
        }
    }
}
