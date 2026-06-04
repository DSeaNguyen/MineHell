using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Boss Settings")]
    public bool autoStart = true;

    [Header("Phase Settings")]
    public List<BossPhase> phases = new List<BossPhase>();

    private int currentPhase = 0;
    private Coroutine activeMoveRoutine = null;
    private Coroutine activeBulletRoutine = null;

    private Health health;

    [Header("Phase Thresholds (%)")]
    public float phase2Threshold = 0.5f;
    public float phase3Threshold = 0.2f;

    private bool phase2Triggered = false;
    private bool phase3Triggered = false;
    private Coroutine phaseRoutine;

    void Start()
    {
        health = GetComponent<Health>();

        if (health == null)
        {
            Debug.LogError("Boss needs Health component!");
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

    void StartPhase(int phaseIndex)
    {
        if (phaseIndex >= phases.Count) return;

        StopAllActivePatterns();

        currentPhase = phaseIndex;

        phaseRoutine = StartCoroutine(RunPhase(phaseIndex));

        Debug.Log("Boss Phase: " + currentPhase);
    }

    void StopAllActivePatterns()
    {
        if (phaseRoutine != null)
        {
            StopCoroutine(phaseRoutine);
            phaseRoutine = null;
        }
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
    }

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
                activeMoveRoutine = StartCoroutine(step.movement.Execute());
            }
            if (step.bulletPattern != null)
            {
                activeBulletRoutine = StartCoroutine(step.bulletPattern.Execute());
            }

            // Wait for both to finish
            if (activeMoveRoutine != null)
            {
                yield return activeMoveRoutine;
                activeMoveRoutine = null;
            }
            if (activeBulletRoutine != null)
            {
                yield return activeBulletRoutine;
                activeBulletRoutine = null;
            }
        }
        else
        {
            // Sequential: movement first, then bullet pattern
            if (step.movement != null)
            {
                activeMoveRoutine = StartCoroutine(step.movement.Execute());
                yield return activeMoveRoutine;
                activeMoveRoutine = null;
            }
            if (step.bulletPattern != null)
            {
                activeBulletRoutine = StartCoroutine(step.bulletPattern.Execute());
                yield return activeBulletRoutine;
                activeBulletRoutine = null;
            }
        }
    }
}