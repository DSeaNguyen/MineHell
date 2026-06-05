using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Boss Settings")]
    public bool autoStart = true;

    [Header("Phase Settings")]
    public List<BossPhase> phases = new List<BossPhase>();

    [Header("Phase Transition")]
    [Tooltip("Seconds to wait between stopping the old phase and starting the new one.")]
    public float phaseTransitionDelay = 1f;

    [Header("Enemy Spawner")]
    [Tooltip("EnemySpawner to activate during Phase 1 only. Leave null if not used.")]
    public EnemySpawner enemySpawner;

    [Header("Phase Thresholds (%)")]
    public float phase2Threshold = 0.5f;
    public float phase3Threshold = 0.2f;

    // --- Active coroutine handles (owned by BossController) ---
    private Coroutine phaseRoutine = null;
    private Coroutine activeMoveRoutine = null;
    private Coroutine activeBulletRoutine = null;

    // --- Active component references (needed to call Stop* during transition) ---
    private BulletPattern activeBulletPattern = null;
    private BossMovement activeMovement = null;

    private int currentPhase = 0;
    private bool phase2Triggered = false;
    private bool phase3Triggered = false;

    private Health health;

    void Start()
    {
        health = GetComponent<Health>();

        if (health == null)
        {
            Debug.LogError("[BossController] Boss is missing a Health component!");
            return;
        }

        if (autoStart && phases.Count > 0)
        {
            StartPhase(0);
        }
    }

    void Update()
    {
        CheckPhase();
    }

    // -------------------------------------------------------------------------
    // Phase Threshold Checking
    // -------------------------------------------------------------------------

    void CheckPhase()
    {
        float hpPercent = (float)health.currentHealth / health.maximumHealth;

        if (!phase2Triggered && hpPercent <= phase2Threshold && phases.Count > 1)
        {
            phase2Triggered = true;
            StartPhase(1);
        }

        if (!phase3Triggered && hpPercent <= phase3Threshold && phases.Count > 2)
        {
            phase3Triggered = true;
            StartPhase(2);
        }
    }

    // -------------------------------------------------------------------------
    // Phase Start / Transition
    // -------------------------------------------------------------------------

    void StartPhase(int phaseIndex)
    {
        if (phaseIndex >= phases.Count) return;

        Debug.Log($"[BossController] Phase transition triggered → Phase {phaseIndex + 1}. Stopping current phase...");

        StopAllActivePatterns();

        currentPhase = phaseIndex;

        // --- Enemy Spawner control ---
        // Spawner is only active during Phase 1 (index 0).
        // Setting enabled = true  fires EnemySpawner.OnEnable → starts SpawnLoop.
        // Setting enabled = false fires EnemySpawner.OnDisable → StopAllCoroutines.
        if (enemySpawner != null)
        {
            bool shouldSpawn = (phaseIndex == 0 || phaseIndex == 1);
            enemySpawner.enabled = shouldSpawn;
            Debug.Log($"[BossController] EnemySpawner {(shouldSpawn ? "enabled" : "disabled")} for Phase {phaseIndex + 1}.");
        }

        // Use a transition wrapper so we can insert the delay before running the phase
        phaseRoutine = StartCoroutine(TransitionAndStartPhase(phaseIndex));
    }

    /// <summary>
    /// Waits for phaseTransitionDelay, then begins RunPhase.
    /// The delay gives cleanup code time to settle (e.g. destroyed GameObjects).
    /// </summary>
    IEnumerator TransitionAndStartPhase(int phaseIndex)
    {
        Debug.Log($"[BossController] Transition delay: {phaseTransitionDelay}s before Phase {phaseIndex + 1}...");
        yield return new WaitForSeconds(phaseTransitionDelay);
        Debug.Log($"[BossController] Starting Phase {phaseIndex + 1}.");
        yield return StartCoroutine(RunPhase(phaseIndex));
    }

    // -------------------------------------------------------------------------
    // Cleanup
    // -------------------------------------------------------------------------

    /// <summary>
    /// Stops all BossController-owned coroutines, then calls StopPattern() and
    /// StopMovement() on whichever components are currently active.
    ///
    /// StopPattern/StopMovement each call StopAllCoroutines() on the pattern's
    /// own MonoBehaviour, then toggle enabled off/on to fire OnDisable cleanup.
    /// </summary>
    void StopAllActivePatterns()
    {
        // 1. Kill the phase-level coroutine chain (RunPhase, TransitionAndStartPhase)
        if (phaseRoutine != null)
        {
            StopCoroutine(phaseRoutine);
            phaseRoutine = null;
        }

        // 2. Kill the step-level coroutine handles owned by BossController
        if (activeMoveRoutine != null)
        {
            StopCoroutine(activeMoveRoutine);
            activeMoveRoutine = null;
        }
        if (activeBulletRoutine != null)
        {
            StopCoroutine(activeBulletRoutine);
            activeBulletRoutine = null;
        }

        // 3. Tell the pattern components to kill THEIR child coroutines and
        //    run their own cleanup (laser destroy, bullet pool return, etc.)
        if (activeBulletPattern != null)
        {
            Debug.Log($"[BossController] Stopping pattern: {activeBulletPattern.GetType().Name}");
            activeBulletPattern.StopPattern();
            activeBulletPattern = null;
        }
        if (activeMovement != null)
        {
            Debug.Log($"[BossController] Stopping movement: {activeMovement.GetType().Name}");
            activeMovement.StopMovement();
            activeMovement = null;
        }
    }

    // -------------------------------------------------------------------------
    // Phase Execution
    // -------------------------------------------------------------------------

    IEnumerator RunPhase(int phaseIndex)
    {
        var phase = phases[phaseIndex];

        while (true)
        {
            foreach (var step in phase.steps)
            {
                yield return StartCoroutine(ExecuteStep(step));
            }
        }
    }

    IEnumerator ExecuteStep(BossActionStep step)
    {
        if (step.executeInParallel)
        {
            activeMoveRoutine = null;
            activeBulletRoutine = null;

            if (step.movement != null)
            {
                activeMovement = step.movement;
                activeMoveRoutine = StartCoroutine(step.movement.Execute());
            }
            if (step.bulletPattern != null)
            {
                activeBulletPattern = step.bulletPattern;
                activeBulletRoutine = StartCoroutine(step.bulletPattern.Execute());
            }

            // Wait for both to finish
            if (activeMoveRoutine != null)
            {
                yield return activeMoveRoutine;
                activeMoveRoutine = null;
                activeMovement = null;
            }
            if (activeBulletRoutine != null)
            {
                yield return activeBulletRoutine;
                activeBulletRoutine = null;
                activeBulletPattern = null;
            }
        }
        else
        {
            // Sequential: movement first, then bullet pattern
            if (step.movement != null)
            {
                activeMovement = step.movement;
                activeMoveRoutine = StartCoroutine(step.movement.Execute());
                yield return activeMoveRoutine;
                activeMoveRoutine = null;
                activeMovement = null;
            }
            if (step.bulletPattern != null)
            {
                activeBulletPattern = step.bulletPattern;
                activeBulletRoutine = StartCoroutine(step.bulletPattern.Execute());
                yield return activeBulletRoutine;
                activeBulletRoutine = null;
                activeBulletPattern = null;
            }
        }
    }
}