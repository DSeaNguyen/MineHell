using System.Collections;
using UnityEngine;

public abstract class BulletPattern : MonoBehaviour
{
    public float duration = 3f;
    public abstract IEnumerator Execute();
}