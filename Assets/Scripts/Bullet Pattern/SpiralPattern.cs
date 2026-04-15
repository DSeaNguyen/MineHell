using System.Collections;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class SpiralPattern : BulletPattern
{
    public GameObject bulletPrefab;
    public float fireRate = 0.05f;
    public float bulletSpeed = 5f;
    public float rotateSpeed = 10f;

    private float angle = 0f;

    public override IEnumerator Execute()
    {
        float timer = 0f;

        while (timer < duration)
        {
            FireSpiral();
            angle += rotateSpeed;
            yield return new WaitForSeconds(fireRate);
            timer += fireRate;
        }
    }

    void FireSpiral()
    {
        float rad = angle * Mathf.Deg2Rad;

        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = dir * bulletSpeed;
    }
}