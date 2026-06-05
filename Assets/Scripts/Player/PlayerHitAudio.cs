using UnityEngine;

/// <summary>
/// Plays a hurt sound clip whenever the player loses HP.
///
/// Polling approach: caches the previous frame's health value and detects
/// a drop as the damage signal. No modification to Health.cs required.
///
/// Duplicate prevention: Health.cs already enforces an invincibility window
/// (invincibilityTime) between hits, so currentHealth can only drop once per
/// window — PlayOneShot will not stack under normal gameplay conditions.
///
/// Setup:
///   1. Attach this script to the Player GameObject.
///   2. Assign the Player's Health component to playerHealth.
///   3. Assign an AudioSource component to audioSource.
///      (Add one to the Player if one does not already exist.)
///   4. Assign your hurt sound AudioClip to hurtClip.
/// </summary>
public class PlayerHitAudio : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Health component on the Player.")]
    public Health playerHealth;

    [Tooltip("The AudioSource used to play the hurt sound.")]
    public AudioSource audioSource;

    [Header("Audio")]
    [Tooltip("The clip to play when the player takes damage.")]
    public AudioClip hurtClip;

    // Cached health value from the previous frame, used to detect a drop
    private int previousHealth;

    void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogWarning("[PlayerHitAudio] playerHealth is not assigned.");
            return;
        }
        if (audioSource == null)
        {
            Debug.LogWarning("[PlayerHitAudio] audioSource is not assigned.");
            return;
        }

        previousHealth = playerHealth.currentHealth;
    }

    void Update()
    {
        if (playerHealth == null || audioSource == null || hurtClip == null) return;

        int currentHealth = playerHealth.currentHealth;

        // A drop in health means damage was successfully applied this frame
        if (currentHealth < previousHealth)
        {
            audioSource.PlayOneShot(hurtClip);
        }

        previousHealth = currentHealth;
    }
}
