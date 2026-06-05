using System.Collections;
using UnityEngine;

public abstract class BulletPattern : MonoBehaviour
{
    public float duration = 3f;

    /// <summary>
    /// Executes the bullet pattern behavior.
    /// Coroutine resolves when the pattern finishes.
    /// </summary>
    public abstract IEnumerator Execute();

    /// <summary>
    /// Stops all coroutines started by this pattern, then toggles the component
    /// disabled/enabled to trigger OnDisable cleanup in subclasses.
    /// Called by BossController during phase transitions.
    /// </summary>
    public virtual void StopPattern()
    {
        StopAllCoroutines();
        enabled = false; // Triggers OnDisable → subclass cleanup
        enabled = true;  // Re-arm for the next phase
    }
}