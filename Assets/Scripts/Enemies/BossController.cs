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
    private List<Coroutine> runningPatterns = new List<Coroutine>();

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

        if (phaseRoutine != null)
        {
            StopCoroutine(phaseRoutine);
        }

        currentPhase = phaseIndex;

        phaseRoutine = StartCoroutine(RunPhase(phaseIndex));

        Debug.Log("Boss Phase: " + currentPhase);
    }

    IEnumerator RunPhase(int phaseIndex)
    {
        var phase = phases[phaseIndex];

        while (true) 
        {
            foreach (var pattern in phase.patterns)
            {
                if (pattern != null)
                {
                    yield return StartCoroutine(pattern.Execute());
                }
            }
        }
    }
}