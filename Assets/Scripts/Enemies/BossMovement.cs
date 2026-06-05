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

    /// <summary>
    /// Stops all coroutines started by this movement, then toggles the component
    /// disabled/enabled to trigger OnDisable cleanup in subclasses.
    /// Called by BossController during phase transitions.
    /// </summary>
    public virtual void StopMovement()
    {
        StopAllCoroutines();
        enabled = false; // Triggers OnDisable → subclass cleanup
        enabled = true;  // Re-arm for the next phase
    }
}
