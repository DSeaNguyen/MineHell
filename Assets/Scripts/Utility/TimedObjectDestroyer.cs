using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TimedObjectDestroyer : MonoBehaviour
{
    [Tooltip("The lifetime of this gameobject")]
    public float lifetime = 5.0f;

    private float timeAlive = 0.0f;

    [Tooltip("Whether to destroy child gameobjects when this gameobject is destroyed")]
    public bool destroyChildrenOnDeath = true;

    public static bool quitting = false;

    private Bullet bullet;

    private void Awake()
    {
        bullet = GetComponent<Bullet>();
    }

    private void OnEnable()
    {
        timeAlive = 0f;
    }

    private void OnApplicationQuit()
    {
        quitting = true;
        DestroyImmediate(this.gameObject);
    }

    void Update()
    {
        timeAlive += Time.deltaTime;

        if (timeAlive > lifetime)
        {
            HandleDeath();
        }
    }

    void HandleDeath()
    {
        if (bullet != null && !quitting)
        {
            bullet.ReturnToPool();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (destroyChildrenOnDeath && !quitting && Application.isPlaying)
        {
            int childCount = transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                GameObject childObject = transform.GetChild(i).gameObject;
                if (childObject != null)
                {
                    DestroyImmediate(childObject);
                }
            }
        }
        transform.DetachChildren();
    }
}