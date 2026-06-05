using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public GameObject bulletPrefab;
    public int initialSize = 50;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Start()
    {
        for (int i = 0; i < initialSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            pool.Enqueue(bullet);
        }
    }

    public GameObject GetBullet()
    {

        if (pool.Count > 0)
        {
            GameObject bullet = pool.Dequeue();
            bullet.SetActive(true);
            return bullet;
        }

        // Pool exhausted: create an overflow bullet and register it with THIS pool
        // so that Bullet.ReturnToPool() correctly re-enqueues it instead of no-oping.
        GameObject overflow = Instantiate(bulletPrefab);
        Bullet overflowScript = overflow.GetComponent<Bullet>();
        if (overflowScript != null)
        {
            overflowScript.SetPool(this);
        }
        return overflow;
    }

    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        pool.Enqueue(bullet);
    }
}