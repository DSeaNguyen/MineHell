using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSweepPattern : BulletPattern
{
    [Header("Bullet Pool Reference")]
    public BulletPool bulletPool;

    [Header("Laser Prefab")]
    [Tooltip("Prefab containing the LaserController, Rigidbody2D (Kinematic), BoxCollider2D, LineRenderer, and Damage component.")]
    public LaserController laserPrefab;

    [Header("Sweep Movement")]
    [Tooltip("The starting position on the left for the laser sweep.")]
    public Transform leftSweepPoint;
    [Tooltip("The ending position on the right for the laser sweep.")]
    public Transform rightSweepPoint;
    [Tooltip("Movement speed during the laser sweep stage.")]
    public float sweepSpeed = 3.0f;
    [Tooltip("Speed when relocating the boss to the start position during Stage 1.")]
    public float repositionSpeed = 5.0f;

    [Header("Laser Config")]
    [Tooltip("Duration of the warning (grey, semi-transparent) phase.")]
    public float warningDuration = 1.5f;
    [Tooltip("Thickness/width of the laser beam.")]
    public float laserWidth = 0.5f;
    [Tooltip("Total vertical length of the laser beam projecting downwards.")]
    public float laserLength = 40.0f;

    [Header("Bullet Rain")]
    [Tooltip("Minimum spawn height for the falling bullets.")]
    public float minBulletSpawnY = 6.0f;
    [Tooltip("Maximum spawn height for the falling bullets.")]
    public float maxBulletSpawnY = 8.0f;
    [Tooltip("Interval delay between successive falling bullet spawns.")]
    public float bulletSpawnInterval = 0.2f;
    [Tooltip("Minimum delay a bullet hovers in place before falling.")]
    public float minFallDelay = 0.3f;
    [Tooltip("Maximum delay a bullet hovers in place before falling.")]
    public float maxFallDelay = 0.8f;
    [Tooltip("Downward fall speed of rain bullets.")]
    public float bulletFallSpeed = 6.0f;

    private LaserController laserInstance;
    private List<GameObject> activeRainBullets = new List<GameObject>();
    private bool isSweeping = false;

    public override IEnumerator Execute()
    {
        if (leftSweepPoint == null || rightSweepPoint == null)
        {
            yield break;
        }

        // --- STAGE 1: Positioning ---
        yield return StartCoroutine(MoveToPosition(leftSweepPoint.position, repositionSpeed));

        // --- STAGE 2: Warning ---
        CreateLaser();
        if (laserInstance != null)
        {
            laserInstance.SetWarning();
        }
        yield return new WaitForSeconds(warningDuration);

        // --- STAGE 3: Sweep ---
        if (laserInstance != null)
        {
            laserInstance.ActivateLaser();
        }
        isSweeping = true;

        // Start bullet rain concurrently
        Coroutine rainCoroutine = StartCoroutine(BulletRainCoroutine());

        // Move boss to the right sweep point
        yield return StartCoroutine(MoveToPosition(rightSweepPoint.position, sweepSpeed));

        // --- STAGE 4: Cleanup ---
        isSweeping = false;
        if (rainCoroutine != null)
        {
            StopCoroutine(rainCoroutine);
        }

        CleanupLaser();

        // Give remaining bullets time to clear the screen
        yield return new WaitForSeconds(1.5f);
        CleanupRainBullets();
    }

    private IEnumerator MoveToPosition(Vector3 targetPos, float speed)
    {
        Vector3 targetCoordinate = new Vector3(targetPos.x, targetPos.y, transform.position.z);
        while (Vector3.Distance(transform.position, targetCoordinate) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetCoordinate, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetCoordinate;
    }

    private void CreateLaser()
    {
        if (laserPrefab == null)
        {
            return;
        }

        // Instantiate laser prefab as a child of the boss transform
        laserInstance = Instantiate(laserPrefab, transform);
        laserInstance.transform.localPosition = Vector3.zero;
        laserInstance.transform.localRotation = Quaternion.identity;

        // Apply width configurations to LineRenderer and BoxCollider
        if (laserInstance.lineRenderer != null)
        {
            laserInstance.lineRenderer.startWidth = laserWidth;
            laserInstance.lineRenderer.endWidth = laserWidth;
        }
        if (laserInstance.boxCollider != null)
        {
            laserInstance.boxCollider.size = new Vector2(laserWidth, laserInstance.boxCollider.size.y);
        }

        // Apply length configuration
        laserInstance.SetLength(laserLength);

        // Configure the Damage parameters for standard player (team 0) interaction
        if (laserInstance.damageComponent != null)
        {
            laserInstance.damageComponent.teamId = 1; // Boss team
            laserInstance.damageComponent.damageAmount = 1;
            laserInstance.damageComponent.dealDamageOnTriggerEnter = true;
            laserInstance.damageComponent.dealDamageOnTriggerStay = true;
            laserInstance.damageComponent.destroyAfterDamage = false; // continuous laser
        }
    }

    private IEnumerator BulletRainCoroutine()
    {
        if (bulletPool == null)
        {
            yield break;
        }

        float minX = Mathf.Min(leftSweepPoint.position.x, rightSweepPoint.position.x);
        float maxX = Mathf.Max(leftSweepPoint.position.x, rightSweepPoint.position.x);

        while (isSweeping)
        {
            float spawnX = Random.Range(minX, maxX);
            float spawnY = Random.Range(minBulletSpawnY, maxBulletSpawnY);
            Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

            float delay = Random.Range(minFallDelay, maxFallDelay);
            StartCoroutine(FallBulletCoroutine(spawnPos, delay, bulletFallSpeed));

            yield return new WaitForSeconds(bulletSpawnInterval);
        }
    }

    private IEnumerator FallBulletCoroutine(Vector3 spawnPos, float delay, float speed)
    {
        GameObject bullet = bulletPool.GetBullet();
        if (bullet == null) yield break;

        bullet.transform.position = spawnPos;
        bullet.transform.rotation = Quaternion.Euler(0, 0, 180f); // point downwards
        activeRainBullets.Add(bullet);

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Init(bulletPool, Vector2.down);
            bulletScript.enabled = false;
        }

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        RigidbodyType2D originalBodyType = RigidbodyType2D.Dynamic;
        if (rb != null)
        {
            originalBodyType = rb.bodyType;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        SpriteRenderer sr = bullet.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = bullet.GetComponentInChildren<SpriteRenderer>();
        }
        Color originalColor = Color.white;
        if (sr != null)
        {
            originalColor = sr.color;
            Color tempColor = originalColor;
            tempColor.a = 0.4f; // warning transparent state
            sr.color = tempColor;
        }

        yield return new WaitForSeconds(delay);

        if (sr != null)
        {
            sr.color = originalColor;
        }

        while (bullet != null && bullet.activeSelf && bullet.transform.position.y > -15f)
        {
            bullet.transform.position += Vector3.down * speed * Time.deltaTime;
            yield return null;
        }

        if (bullet != null && bullet.activeSelf)
        {
            if (bulletScript != null)
            {
                bulletScript.enabled = true;
            }
            if (rb != null)
            {
                rb.bodyType = originalBodyType;
            }
            bulletScript.ReturnToPool();
        }
    }

    private void Update()
    {
        for (int i = activeRainBullets.Count - 1; i >= 0; i--)
        {
            if (activeRainBullets[i] == null || !activeRainBullets[i].activeSelf)
            {
                activeRainBullets.RemoveAt(i);
            }
        }
    }

    private void OnDisable()
    {
        CleanupLaser();
        CleanupRainBullets();
    }

    private void OnDestroy()
    {
        CleanupLaser();
        CleanupRainBullets();
    }

    private void CleanupLaser()
    {
        if (laserInstance != null)
        {
            Destroy(laserInstance.gameObject);
            laserInstance = null;
        }
    }

    private void CleanupRainBullets()
    {
        for (int i = activeRainBullets.Count - 1; i >= 0; i--)
        {
            GameObject bullet = activeRainBullets[i];
            if (bullet != null)
            {
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.enabled = true;
                }
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;
                }
                bullet.transform.SetParent(null);

                if (bullet.activeSelf)
                {
                    if (bulletScript != null)
                    {
                        bulletScript.ReturnToPool();
                    }
                    else
                    {
                        bullet.SetActive(false);
                    }
                }
            }
        }
        activeRainBullets.Clear();
    }
}
