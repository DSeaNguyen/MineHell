using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserGridPattern : BulletPattern
{
    [Header("Laser Prefabs")]
    [Tooltip("Prefab for the moving horizontal laser (travels Top -> Bottom).")]
    public LaserController horizontalLaserPrefab;
    [Tooltip("Prefab for the moving vertical laser (travels Left -> Right).")]
    public LaserController verticalLaserPrefab;

    [Header("Arena Bounds")]
    [Tooltip("Transform defining the top boundary for horizontal laser spawning.")]
    public Transform topSpawnPoint;
    [Tooltip("Transform defining the bottom boundary for horizontal laser target destination.")]
    public Transform bottomTargetPoint;
    [Tooltip("Transform defining the left boundary for vertical laser spawning.")]
    public Transform leftSpawnPoint;
    [Tooltip("Transform defining the right boundary for vertical laser target destination.")]
    public Transform rightTargetPoint;

    [Header("Grid Timing")]
    [Tooltip("Time window in seconds during which new laser pairs continue spawning.")]
    public float spawnDuration = 6.0f;
    [Tooltip("Time in seconds it takes for each laser to travel across the arena.")]
    public float travelTime = 3.0f;
    [Tooltip("Delay in seconds between successive laser pair spawns.")]
    public float spawnInterval = 1.0f;

    private List<LaserController> activeLasers = new List<LaserController>();

    public override IEnumerator Execute()
    {
        if (horizontalLaserPrefab == null || verticalLaserPrefab == null)
        {
            yield break;
        }

        if (topSpawnPoint == null || bottomTargetPoint == null || leftSpawnPoint == null || rightTargetPoint == null)
        {
            yield break;
        }

        float elapsed = 0f;

        // Continuously spawn laser pairs until the duration expires
        while (elapsed < spawnDuration)
        {
            SpawnLaserPair();
            yield return new WaitForSeconds(spawnInterval);
            elapsed += spawnInterval;
        }

        // Wait for all currently active lasers to finish traveling before ending the pattern step
        yield return new WaitForSeconds(travelTime);
    }

    private void SpawnLaserPair()
    {
        float zPos = transform.position.z;
        float arenaWidth = rightTargetPoint.position.x - leftSpawnPoint.position.x;
        float arenaHeight = topSpawnPoint.position.y - bottomTargetPoint.position.y;

        // --- 1. HORIZONTAL LASER (Moves Top -> Bottom, projects Left -> Right) ---
        Vector3 hStart = new Vector3(leftSpawnPoint.position.x, topSpawnPoint.position.y, zPos);
        Vector3 hTarget = new Vector3(leftSpawnPoint.position.x, bottomTargetPoint.position.y, zPos);

        LaserController hLaser = Instantiate(horizontalLaserPrefab);
        hLaser.transform.position = hStart;
        // Rotate by 90 degrees so that the local downwards beam projects to the right (horizontally)
        hLaser.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        hLaser.SetLength(arenaWidth);
        hLaser.ActivateLaser();

        // Configure team ID to enemy (1) to trigger damage on Player (0)
        if (hLaser.damageComponent != null)
        {
            hLaser.damageComponent.teamId = 1;
            hLaser.damageComponent.damageAmount = 1;
            hLaser.damageComponent.dealDamageOnTriggerEnter = true;
            hLaser.damageComponent.dealDamageOnTriggerStay = true;
            hLaser.damageComponent.destroyAfterDamage = false;
        }

        activeLasers.Add(hLaser);
        StartCoroutine(TravelLaserCoroutine(hLaser, hStart, hTarget));

        // --- 2. VERTICAL LASER (Moves Left -> Right, projects Top -> Bottom) ---
        Vector3 vStart = new Vector3(leftSpawnPoint.position.x, topSpawnPoint.position.y, zPos);
        Vector3 vTarget = new Vector3(rightTargetPoint.position.x, topSpawnPoint.position.y, zPos);

        LaserController vLaser = Instantiate(verticalLaserPrefab);
        vLaser.transform.position = vStart;
        vLaser.transform.rotation = Quaternion.identity; // Default points downwards (vertically)
        vLaser.SetLength(arenaHeight);
        vLaser.ActivateLaser();

        if (vLaser.damageComponent != null)
        {
            vLaser.damageComponent.teamId = 1;
            vLaser.damageComponent.damageAmount = 1;
            vLaser.damageComponent.dealDamageOnTriggerEnter = true;
            vLaser.damageComponent.dealDamageOnTriggerStay = true;
            vLaser.damageComponent.destroyAfterDamage = false;
        }

        activeLasers.Add(vLaser);
        StartCoroutine(TravelLaserCoroutine(vLaser, vStart, vTarget));
    }

    private IEnumerator TravelLaserCoroutine(LaserController laser, Vector3 start, Vector3 target)
    {
        float elapsed = 0f;

        while (elapsed < travelTime)
        {
            if (laser == null) yield break;

            elapsed += Time.deltaTime;
            float percent = Mathf.Clamp01(elapsed / travelTime);
            laser.transform.position = Vector3.Lerp(start, target, percent);
            yield return null;
        }

        if (laser != null)
        {
            activeLasers.Remove(laser);
            Destroy(laser.gameObject);
        }
    }

    private void Update()
    {
        // Keep active list clean of already destroyed components
        for (int i = activeLasers.Count - 1; i >= 0; i--)
        {
            if (activeLasers[i] == null)
            {
                activeLasers.RemoveAt(i);
            }
        }
    }

    private void OnDisable()
    {
        CleanupLasers();
    }

    private void OnDestroy()
    {
        CleanupLasers();
    }

    private void CleanupLasers()
    {
        foreach (var laser in activeLasers)
        {
            if (laser != null)
            {
                Destroy(laser.gameObject);
            }
        }
        activeLasers.Clear();
    }
}
