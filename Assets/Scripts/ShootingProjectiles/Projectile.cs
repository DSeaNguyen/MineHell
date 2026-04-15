using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float projectileSpeed = 3.0f;

    private BulletPool pool;

    public void Init(BulletPool poolRef)
    {
        pool = poolRef;
    }

    private void OnEnable()
    {
        
    }

    private void Update()
    {
        MoveProjectile();
    }

    private void MoveProjectile()
    {
        transform.position += transform.up * projectileSpeed * Time.deltaTime;
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