using UnityEngine;

public class LaserController : MonoBehaviour
{
    [Header("Component References")]
    [Tooltip("Line renderer used to display the laser visuals.")]
    public LineRenderer lineRenderer;

    [Tooltip("Collider component used to detect collisions with the player.")]
    public BoxCollider2D boxCollider;

    [Tooltip("Damage handler that inflicts damage on trigger contacts.")]
    public Damage damageComponent;

    [Header("Visual Configurations")]
    [Tooltip("Color of the laser during the telegraph/warning phase.")]
    public Color warningColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);

    [Tooltip("Color of the laser when actively dealing damage.")]
    public Color activeColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);

    /// <summary>
    /// Sets the laser to a semi-transparent warning state and disables damage collision.
    /// </summary>
    public void SetWarning()
    {
        if (lineRenderer != null)
        {
            lineRenderer.startColor = warningColor;
            lineRenderer.endColor = warningColor;
        }

        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }
    }

    /// <summary>
    /// Activates the laser to its full opaque state and enables damage collision.
    /// </summary>
    public void ActivateLaser()
    {
        if (lineRenderer != null)
        {
            lineRenderer.startColor = activeColor;
            lineRenderer.endColor = activeColor;
        }

        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }
    }

    /// <summary>
    /// Adjusts the LineRenderer positions and BoxCollider size/offset relative to the laser length.
    /// </summary>
    /// <param name="length">Length of the laser projecting downwards.</param>
    public void SetLength(float length)
    {
        if (lineRenderer != null)
        {
            lineRenderer.useWorldSpace = false;
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, new Vector3(0f, -length, 0f));
        }

        if (boxCollider != null)
        {
            boxCollider.size = new Vector2(boxCollider.size.x, length);
            boxCollider.offset = new Vector2(0f, -length / 2f);
        }
    }
}
