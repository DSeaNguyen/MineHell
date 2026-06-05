using UnityEngine;

/// <summary>
/// Briefly flashes the player's sprite red when damage is taken,
/// then restores the original color.
///
/// Polling approach: caches the previous frame's health and detects a drop.
/// Timer-driven flash: uses a float countdown in Update() instead of a
/// Coroutine — zero allocation per hit, no coroutine overhead.
///
/// Setup:
///   1. Attach this script to the Player GameObject.
///   2. Assign the Player's Health component to playerHealth.
///   3. Assign the Player's SpriteRenderer to spriteRenderer.
///   4. Tune flashColor and flashDuration in the Inspector.
/// </summary>
public class PlayerHitFlash : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Health component on the Player.")]
    public Health playerHealth;

    [Tooltip("The SpriteRenderer whose color will be flashed.")]
    public SpriteRenderer spriteRenderer;

    [Header("Flash Settings")]
    [Tooltip("Color to flash to when the player takes damage.")]
    public Color flashColor = new Color(1f, 0.15f, 0.15f, 1f);

    [Tooltip("How long the flash lasts in seconds.")]
    public float flashDuration = 0.15f;

    // Cached health value from the previous frame
    private int previousHealth;

    // The sprite's original color, captured once at Start
    private Color originalColor;

    // Countdown timer; when > 0 the flash is active
    private float flashTimer = 0f;

    void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogWarning("[PlayerHitFlash] playerHealth is not assigned.");
            return;
        }
        if (spriteRenderer == null)
        {
            Debug.LogWarning("[PlayerHitFlash] spriteRenderer is not assigned.");
            return;
        }

        previousHealth = playerHealth.currentHealth;
        originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (playerHealth == null || spriteRenderer == null) return;

        int currentHealth = playerHealth.currentHealth;

        // Damage detected — restart the flash timer
        if (currentHealth < previousHealth)
        {
            flashTimer = flashDuration;
            spriteRenderer.color = flashColor;
        }

        previousHealth = currentHealth;

        // Tick down the flash timer
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;

            if (flashTimer <= 0f)
            {
                // Timer expired — restore original color
                flashTimer = 0f;
                spriteRenderer.color = originalColor;
            }
        }
    }

    /// <summary>
    /// Restores the sprite color immediately if the component is disabled
    /// mid-flash (e.g. on scene unload or player death).
    /// </summary>
    private void OnDisable()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        flashTimer = 0f;
    }
}
