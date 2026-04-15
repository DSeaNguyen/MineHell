using System.Collections;
using UnityEngine;

/// <summary>
/// Spawn enemy xung quanh boss
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform target;

    public float spawnRangeX = 10f;
    public float spawnRangeY = 10f;

    public int enemiesPerWave = 3;
    public float spawnInterval = 10f;
    public int maxSpawn = 0; // 0 = vô hạn
    public bool spawnInfinite = true;

    private int currentlySpawned = 0;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
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

            // Nếu màn boss → quái phụ không tính điểm
            if (GameManager.instance != null && GameManager.instance.isBossLevel && !enemy.isBoss)
            {
                enemy.scoreValue = 0;
            }
        }
    }
}
