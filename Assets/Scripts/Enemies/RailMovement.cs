using System.Collections;
using UnityEngine;

public class RailMovement : BossMovement
{
    [Header("Rail Settings")]
    [Tooltip("The locked Y position of the horizontal rail.")]
    public float railY = 3.0f;

    [Tooltip("Minimum X boundary constraint.")]
    public float minX = -8.0f;

    [Tooltip("Maximum X boundary constraint.")]
    public float maxX = 8.0f;

    [Header("Tracking Control")]
    [Tooltip("How fast the boss matches the player's position (using Lerp).")]
    public float followSpeed = 2.0f;

    [Tooltip("Time delay (seconds) before player movement registers on the boss's tracking target.")]
    public float trackingDelay = 0.5f;

    // History buffer to track player position over time
    private struct PositionLog
    {
        public float time;
        public float xPosition;
    }
    private System.Collections.Generic.Queue<PositionLog> positionHistory = new System.Collections.Generic.Queue<PositionLog>();

    public override IEnumerator Execute()
    {
        float timer = 0f;

        // Align Y position immediately when starting rail movement
        Vector3 startPos = transform.position;
        startPos.y = railY;
        transform.position = startPos;

        // Clear history
        positionHistory.Clear();

        while (timer < duration)
        {
            float dt = Time.deltaTime;
            timer += dt;

            Transform player = GetPlayerTransform();
            if (player != null)
            {
                // Log current player X position
                positionHistory.Enqueue(new PositionLog { time = Time.time, xPosition = player.position.x });

                // Retrieve position from history that corresponds to (currentTime - trackingDelay)
                float targetX = player.position.x;
                while (positionHistory.Count > 0 && Time.time - positionHistory.Peek().time > trackingDelay)
                {
                    targetX = positionHistory.Dequeue().xPosition;
                }

                // Clamp target X to boundary limits
                targetX = Mathf.Clamp(targetX, minX, maxX);

                // Move towards target X smoothly
                float currentX = transform.position.x;
                float nextX = Mathf.Lerp(currentX, targetX, followSpeed * dt);
                transform.position = new Vector3(nextX, railY, transform.position.z);
            }

            yield return null;
        }
    }

    private Transform GetPlayerTransform()
    {
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            return GameManager.instance.player.transform;
        }
        Controller ctrl = FindObjectOfType<Controller>();
        return ctrl != null ? ctrl.transform : null;
    }
}
