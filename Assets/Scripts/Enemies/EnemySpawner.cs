using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns enemy waves around the boss.
/// 
/// Lifecycle is controlled externally by BossController:
///   - BossController sets enabled = true  → OnEnable starts the spawn loop.
///   - BossController sets enabled = false → OnDisable stops the spawn loop immediately.
/// 
/// EnemySpawner itself has no phase awareness and performs no per-frame checks.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform target;

    public float spawnRangeX = 10f;
    public float spawnRangeY = 10f;

    public int enemiesPerWave = 3;
    public float spawnInterval = 10f;
    public int maxSpawn = 0;        // 0 = unlimited
    public bool spawnInfinite = true;

    private int currentlySpawned = 0;

    /// <summary>
    /// Called whenever this component is enabled.
    /// BossController enables this component to begin Phase 1 spawning.
    /// </summary>
    private void OnEnable()
    {
        StartCoroutine(SpawnLoop());
    }

    /// <summary>
    /// Called whenever this component is disabled.
    /// BossController disables this component on Phase 2+ to halt all future spawns.
    /// StopAllCoroutines kills the SpawnLoop immediately, even mid-interval.
    /// Existing spawned enemies are unaffected.
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (spawnInfinite || maxSpawn == 0 || currentlySpawned < maxSpawn)
            {
                SpawnWave();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            if (maxSpawn > 0 && currentlySpawned >= maxSpawn) break;
            Vector3 spawnPos = GetSpawnLocation();
            SpawnEnemy(spawnPos);
        }
    }

    private Vector3 GetSpawnLocation()
    {
        float x = Random.Range(-spawnRangeX, spawnRangeX);
        float y = Random.Range(-spawnRangeY, spawnRangeY);
        return transform.position + new Vector3(x, y, 0);
    }

    private void SpawnEnemy(Vector3 pos)
    {
        if (enemyPrefab == null) return;

        GameObject enemyObj = Instantiate(enemyPrefab, pos, enemyPrefab.transform.rotation);
        currentlySpawned++;

        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.followTarget = target;

            // Boss level sub-enemies don't award score
            if (GameManager.instance != null && GameManager.instance.isBossLevel && !enemy.isBoss)
            {
                enemy.scoreValue = 0;
            }
        }
    }
}
