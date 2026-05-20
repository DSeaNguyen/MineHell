using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public bool useRigidbody = true;

    private Rigidbody2D rb;

    [Header("Lifetime")]
    public float lifetime = 3f;
    private float timer;

    private BulletPool pool;
    private bool isInitialized = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(BulletPool poolRef, Vector2 direction)
    {
        pool = poolRef;
        isInitialized = true;

        timer = 0f;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (useRigidbody && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.linearVelocity = direction * speed;
        }
    }

    void OnEnable()
    {
        
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (!useRigidbody)
        {
            transform.position += transform.up * speed * Time.deltaTime;
        }

        if (!isInitialized) return;


        if (timer >= lifetime)
        {
            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        isInitialized = false;

        if (pool != null)
        {
            pool.ReturnBullet(gameObject);
        }
    }
}