using System.Collections;
using UnityEngine;

public abstract class BossMovement : MonoBehaviour
{
    [Tooltip("How long this movement behavior should run/persist")]
    public float duration = 3f;

    /// <summary>
    /// Executes the movement behavior.
    /// Coroutine resolves when the movement phase finishes.
    /// </summary>
    public abstract IEnumerator Execute();
}
