using System.Collections;
using UnityEngine;

public class SpiralPattern : BulletPattern
{
    public BulletPool spiralPool;

    public float fireRate = 0.05f;
    public float bulletSpeed = 5f;
    public float rotateSpeed = 10f;

    public int streamCount = 1;

    private float angle = 0f;

    public override IEnumerator Execute()
    {
        float timer = 0f;

        while (timer < duration)
        {
            FireSpiral();

            yield return new WaitForSeconds(fireRate);

            timer += fireRate;
        }
    }

    void FireSpiral()
    {
        float angleStep = 360f / streamCount;

        for (int i = 0; i < streamCount; i++)
        {
            float currentAngle = angle + (i * angleStep);

            float rad = currentAngle * Mathf.Deg2Rad;

            Vector2 dir = new Vector2(
                Mathf.Cos(rad),
                Mathf.Sin(rad)
            );

            GameObject bullet = spiralPool.GetBullet();

            bullet.transform.position = transform.position;
            bullet.transform.rotation = Quaternion.identity;

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

            rb.linearVelocity = Vector2.zero;
            rb.linearVelocity = dir * bulletSpeed;

            bullet.GetComponent<Bullet>().Init(spiralPool, dir);
        }

        angle += rotateSpeed;
    }
}