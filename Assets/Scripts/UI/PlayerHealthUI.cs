using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Keeps a UI Slider synchronized with a player's Health component.
/// 
/// Polling approach: reads currentHealth every frame and updates the slider.
/// No modification to Health.cs required.
/// 
/// Setup:
///   1. Attach this script to a GameObject in your HUD Canvas.
///   2. Assign the Player's Health component to playerHealth.
///   3. Assign a UI Slider to healthSlider.
///   4. (Optional) style the slider Fill and Background in the Inspector.
/// </summary>
public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Health component on the Player.")]
    public Health playerHealth;

    [Tooltip("The UI Slider that represents the health bar.")]
    public Slider healthSlider;

    void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogWarning("[PlayerHealthUI] playerHealth is not assigned.");
            return;
        }
        if (healthSlider == null)
        {
            Debug.LogWarning("[PlayerHealthUI] healthSlider is not assigned.");
            return;
        }

        // Set the slider range once at startup to match the player's maximum HP
        healthSlider.maxValue = playerHealth.maximumHealth;
        healthSlider.minValue = 0;

        // Initialise to current value immediately so there is no one-frame flash
        healthSlider.value = playerHealth.currentHealth;
    }

    void Update()
    {
        if (playerHealth == null || healthSlider == null) return;

        // Poll and sync every frame.
        // Unity's Slider setter is a no-op when the value hasn't changed, so
        // this is cheaper than it looks.
        healthSlider.value = playerHealth.currentHealth;
    }
}
