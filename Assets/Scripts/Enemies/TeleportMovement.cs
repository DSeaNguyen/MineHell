using System.Collections;
using UnityEngine;

public class TeleportMovement : BossMovement
{
    [Header("Teleport Settings")]
    [Tooltip("Prefab to spawn as the visual warning/telegraph indicator.")]
    public GameObject telegraphPrefab;

    [Tooltip("Delay in seconds between spawning the telegraph and performing the teleport.")]
    public float telegraphDuration = 1.0f;

    [Header("Arena Boundaries")]
    [Tooltip("Minimum X and Y boundaries for the random teleport location.")]
    public Vector2 minBounds = new Vector2(-8f, -4f);

    [Tooltip("Maximum X and Y boundaries for the random teleport location.")]
    public Vector2 maxBounds = new Vector2(8f, 4f);

    private GameObject activeTelegraph = null;

    public override IEnumerator Execute()
    {
        // Determine a random position in the arena bounds
        float randomX = Random.Range(minBounds.x, maxBounds.x);
        float randomY = Random.Range(minBounds.y, maxBounds.y);
        Vector3 destination = new Vector3(randomX, randomY, transform.position.z);

        if (telegraphPrefab != null)
        {
            activeTelegraph = Instantiate(telegraphPrefab, destination, Quaternion.identity);
        }

        // Wait for telegraph duration
        yield return new WaitForSeconds(telegraphDuration);

        // Teleport
        transform.position = destination;

        CleanupTelegraph();

        // Wait for the remainder of the movement duration
        float remainder = duration - telegraphDuration;
        if (remainder > 0f)
        {
            yield return new WaitForSeconds(remainder);
        }
    }

    private void OnDisable()
    {
        CleanupTelegraph();
    }

    private void OnDestroy()
    {
        CleanupTelegraph();
    }

    private void CleanupTelegraph()
    {
        if (activeTelegraph != null)
        {
            Destroy(activeTelegraph);
            activeTelegraph = null;
        }
    }
}
