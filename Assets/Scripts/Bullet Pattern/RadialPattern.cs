using System.Collections;
using UnityEngine;

public class RadialPattern : BulletPattern
{
    public BulletPool radialPool;

    public int bulletCount = 20;
    public float fireRate = 1f;
    public float bulletSpeed = 5f;
    public float rotateSpeed = 10f;

    private float angleOffset = 0f;

    public override IEnumerator Execute()
    {
        float timer = 0f;

        while (timer < duration)
        {
            FireRadial();
            yield return new WaitForSeconds(fireRate);
            timer += fireRate;
        }
    }

    void FireRadial()
    {
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = (i * angleStep + angleOffset) * Mathf.Deg2Rad;

            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            GameObject bullet = radialPool.GetBullet();

            bullet.transform.position = transform.position;
            bullet.transform.rotation = Quaternion.identity;

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

            rb.linearVelocity = Vector2.zero;
            rb.linearVelocity = dir * bulletSpeed;

            bullet.GetComponent<Bullet>().Init(radialPool, dir);
        }

        angleOffset += rotateSpeed;
    }
}