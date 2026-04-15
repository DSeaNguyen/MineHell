using System.Collections;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class RadialPattern : BulletPattern
{
    public GameObject bulletPrefab;
    public int bulletCount = 20;
    public float fireRate = 1f;
    public float bulletSpeed = 5f;
    public float rotateSpeed = 10f;

    private float angleOffset = 1f;

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

            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullet.GetComponent<Rigidbody2D>().linearVelocity = dir * bulletSpeed;
        }

        angleOffset += rotateSpeed;
    }
}