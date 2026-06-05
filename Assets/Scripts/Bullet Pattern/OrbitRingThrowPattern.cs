using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitRingThrowPattern : BulletPattern
{
    [Header("Pool Reference")]
    public BulletPool bulletPool;

    [Header("Layout Settings")]
    [Tooltip("Distance from the boss to spawn each of the 4 ring centers.")]
    public float spawnRadius = 3.0f;

    [Tooltip("Radius of the circle formed by the 6 bullets in each ring.")]
    public float bulletCircleRadius = 0.8f;

    [Tooltip("Number of bullets per ring.")]
    public int bulletsPerRing = 6;

    [Header("Timings & Speeds")]
    [Tooltip("Duration of the fade-in effect for each ring.")]
    public float fadeDuration = 0.4f;

    [Tooltip("Target rotation speed of the rings around their center.")]
    public float maxRotationSpeed = 360f;

    [Tooltip("Travel speed of the rings after being thrown.")]
    public float throwSpeed = 8.0f;

    // Semicircle angles above the boss: left to right
    private readonly float[] ringAngles = { 180f, 135f, 45f, 0f };
    private readonly float[] summonTimes = { 0.0f, 0.2f, 0.4f, 0.6f };
    private readonly float[] throwTimes = { 1.8f, 2.1f, 2.4f, 2.7f };

    private RingController[] rings = new RingController[4];
    private float patternStartTime;

    public override IEnumerator Execute()
    {
        patternStartTime = Time.time;

        // Summon rings sequentially
        for (int i = 0; i < 4; i++)
        {
            float delay = summonTimes[i];
            StartCoroutine(SummonRingCoroutine(i, delay));
        }

        // Wait for pattern duration (approx 3.5 seconds to allow the final thrown ring to travel)
        yield return new WaitForSeconds(duration);
    }

    private IEnumerator SummonRingCoroutine(int index, float delay)
    {
        yield return new WaitForSeconds(delay);

        float angle = ringAngles[index];
        Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0) * spawnRadius;
        Vector3 spawnPos = transform.position + offset;

        // Create the Ring Container parent
        GameObject container = new GameObject($"RingContainer_{index}");
        container.transform.position = spawnPos;

        // Attach and initialize the RingController
        RingController ringCtrl = container.AddComponent<RingController>();
        ringCtrl.Init(bulletPool, bulletsPerRing, bulletCircleRadius, fadeDuration);
        rings[index] = ringCtrl;

        // Listen for the throw timing
        StartCoroutine(ThrowCheckCoroutine(index, ringCtrl));
    }

    private IEnumerator ThrowCheckCoroutine(int index, RingController ring)
    {
        float targetThrowTime = patternStartTime + throwTimes[index];
        float timeToWait = targetThrowTime - Time.time;
        if (timeToWait > 0f)
        {
            yield return new WaitForSeconds(timeToWait);
        }

        if (ring != null)
        {
            // Aim at player's current coordinates
            Transform player = GetPlayerTransform();
            Vector3 targetDir = transform.up; // default fallback if player is missing
            if (player != null)
            {
                targetDir = (player.position - ring.transform.position).normalized;
            }

            ring.Throw(targetDir, throwSpeed);
        }
    }

    private Transform GetPlayerTransform()
    {
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            return GameManager.instance.player.transform;
        }
        Controller ctrl = FindObjectOfType<Controller>();
        return ctrl != null ? ctrl.transform : null;
    }

    private void Update()
    {
        if (rings == null) return;

        float elapsed = Time.time - patternStartTime;

        // Handle rotation speed acceleration
        float rotSpeed = 0f;
        if (elapsed >= 0.5f)
        {
            // Accelerate speed gradually between 0.5s and 2.0s
            float progress = Mathf.Clamp01((elapsed - 0.5f) / 1.5f);
            rotSpeed = Mathf.Lerp(0f, maxRotationSpeed, progress);
        }

        for (int i = 0; i < 4; i++)
        {
            if (rings[i] != null)
            {
                if (!rings[i].isThrown)
                {
                    rings[i].rotationSpeed = rotSpeed;
                }
                else
                {
                    // Thrown rings continue rotating at maximum velocity
                    rings[i].rotationSpeed = maxRotationSpeed;
                }
            }
        }
    }

    private void OnDisable()
    {
        CleanupRings();
    }

    private void OnDestroy()
    {
        CleanupRings();
    }

    /// <summary>
    /// Destroys all live ring container GameObjects.
    /// RingController.OnDestroy handles returning bullets to the pool.
    /// </summary>
    private void CleanupRings()
    {
        if (rings == null) return;

        for (int i = 0; i < rings.Length; i++)
        {
            if (rings[i] != null)
            {
                Destroy(rings[i].gameObject);
                rings[i] = null;
            }
        }
    }
}

public class RingController : MonoBehaviour
{
    public float rotationSpeed = 0f;
    public bool isThrown { get; private set; } = false;

    private Vector3 moveDirection;
    private float moveSpeed;

    private List<GameObject> bulletList = new List<GameObject>();
    private List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
    private List<RigidbodyType2D> originalBodyTypes = new List<RigidbodyType2D>();

    private float appearanceTime;
    private float fadeDuration;

    public void Init(BulletPool pool, int count, float radius, float fadeTime)
    {
        appearanceTime = Time.time;
        fadeDuration = fadeTime;

        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 localOffset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;

            GameObject bullet = pool.GetBullet();
            if (bullet != null)
            {
                bullet.transform.position = transform.position + localOffset;
                bullet.transform.rotation = Quaternion.identity;
                bullet.transform.SetParent(transform);

                bulletList.Add(bullet);

                // Disable Bullet script movement
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.Init(pool, localOffset.normalized);
                    bulletScript.enabled = false;
                }

                // Configure Rigidbody2D to kinematic so it translates with container parent
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    originalBodyTypes.Add(rb.bodyType);
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
                else
                {
                    originalBodyTypes.Add(RigidbodyType2D.Dynamic);
                }

                // Cache sprite for transparent fade-ins
                SpriteRenderer sr = bullet.GetComponent<SpriteRenderer>();
                if (sr == null)
                {
                    sr = bullet.GetComponentInChildren<SpriteRenderer>();
                }
                if (sr != null)
                {
                    spriteRenderers.Add(sr);
                    Color c = sr.color;
                    c.a = 0f;
                    sr.color = c;
                }
                else
                {
                    spriteRenderers.Add(null);
                }
            }
        }
    }

    private void Update()
    {
        // 1. Transparent fade-in processing
        float elapsed = Time.time - appearanceTime;
        if (elapsed < fadeDuration)
        {
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            SetBulletsAlpha(alpha);
        }
        else
        {
            SetBulletsAlpha(1f);
        }

        // 2. Rotate container around its own center
        if (rotationSpeed > 0f)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }

        // 3. Translate container in aim direction if thrown
        if (isThrown)
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }

        // 4. Integrity check (clean up inactive bullets returned to pool mid-flight)
        for (int i = bulletList.Count - 1; i >= 0; i--)
        {
            GameObject bullet = bulletList[i];
            if (bullet == null || !bullet.activeSelf)
            {
                if (bullet != null)
                {
                    RestoreBulletState(bullet, i);
                    bullet.transform.SetParent(null);
                }
                bulletList.RemoveAt(i);
                spriteRenderers.RemoveAt(i);
                originalBodyTypes.RemoveAt(i);
            }
        }

        // 5. Auto-destroy container when all bullets are gone
        if (bulletList.Count == 0 && isThrown)
        {
            Destroy(gameObject);
        }
    }

    public void Throw(Vector3 direction, float speed)
    {
        moveDirection = direction;
        moveSpeed = speed;
        isThrown = true;
        SetBulletsAlpha(1f);
    }

    private void SetBulletsAlpha(float alpha)
    {
        for (int i = 0; i < spriteRenderers.Count; i++)
        {
            if (spriteRenderers[i] != null)
            {
                Color c = spriteRenderers[i].color;
                c.a = alpha;
                spriteRenderers[i].color = c;
            }
        }
    }

    private void RestoreBulletState(GameObject bullet, int index)
    {
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.enabled = true;
        }

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null && index < originalBodyTypes.Count)
        {
            rb.bodyType = originalBodyTypes[index];
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnDisable()
    {
        CleanupRemainingBullets();
    }

    private void OnDestroy()
    {
        CleanupRemainingBullets();
    }

    private void CleanupRemainingBullets()
    {
        // Safe pool return to prevent memory leaks on phase transition/boss death
        for (int i = bulletList.Count - 1; i >= 0; i--)
        {
            GameObject bullet = bulletList[i];
            if (bullet != null)
            {
                RestoreBulletState(bullet, i);
                bullet.transform.SetParent(null);
                if (bullet.activeSelf)
                {
                    Bullet bScript = bullet.GetComponent<Bullet>();
                    if (bScript != null)
                    {
                        bScript.ReturnToPool();
                    }
                    else
                    {
                        bullet.SetActive(false);
                    }
                }
            }
        }
        bulletList.Clear();
        spriteRenderers.Clear();
        originalBodyTypes.Clear();
    }
}
