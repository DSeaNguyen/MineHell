using UnityEngine;

public class Bullet : MonoBehaviour
{
    private BulletPool pool;

    public void Init(BulletPool poolRef)
    {
        pool = poolRef;
    }

    public void ReturnToPool()
    {
        if (pool != null)
        {
            pool.ReturnBullet(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}