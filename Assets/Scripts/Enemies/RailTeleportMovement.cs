using System.Collections;
using UnityEngine;

public class RailTeleportMovement : BossMovement
{
    [Header("Rail Bounds")]
    [Tooltip("Left boundary of the teleportation rail.")]
    public Transform railLeft;

    [Tooltip("Right boundary of the teleportation rail.")]
    public Transform railRight;

    [Header("Teleport Loop Config")]
    [Tooltip("Number of times the boss teleports in this step.")]
    public int teleportCount = 3;

    [Tooltip("Delay in seconds between successive teleport bursts.")]
    public float delayBetweenTeleports = 0.4f;

    [Header("Attacks")]
    [Tooltip("Radial bullet pattern component used to fire single ring bursts.")]
    public RadialPattern radialPattern;

    public override IEnumerator Execute()
    {
        if (railLeft == null || railRight == null)
        {
            Debug.LogError("RailTeleportMovement: Left and Right rail boundary transforms must be assigned!");
            yield break;
        }

        if (radialPattern == null)
        {
            Debug.LogError("RailTeleportMovement: RadialPattern component must be assigned!");
            yield break;
        }

        float minX = Mathf.Min(railLeft.position.x, railRight.position.x);
        float maxX = Mathf.Max(railLeft.position.x, railRight.position.x);
        float railY = railLeft.position.y; // horizontal rail coordinate

        for (int i = 0; i < teleportCount; i++)
        {
            // 1. Choose random X
            float targetX = Random.Range(minX, maxX);

            // 2. Instantly teleport
            transform.position = new Vector3(targetX, railY, transform.position.z);

            // 3. Trigger single burst
            radialPattern.FireSingleRing();

            // 4. Wait for delay
            yield return new WaitForSeconds(delayBetweenTeleports);
        }
    }
}
