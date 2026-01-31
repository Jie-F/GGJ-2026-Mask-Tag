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

    [Header("Stuck Detection")]
    public float stuckCheckInterval = 0.5f;
    public float stuckTimeThreshold = 3f;
    public float minMovementDistance = 0.1f;

    [Header("Teleport Recovery")]
    public float teleportRadius = 4f;
    public float teleportHeight = 5f;
    public int teleportAttempts = 10;

    private Vector3 lastPosition;
    private float stuckTimer;
    private float checkTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            player = playerObj.transform;
        }

        lastPosition = transform.position;

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

        CheckIfStuck();
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
                return;
            }
        }

        UnityEngine.Debug.LogError("Enemy teleport recovery failed — no valid NavMesh point found");
    }
}
