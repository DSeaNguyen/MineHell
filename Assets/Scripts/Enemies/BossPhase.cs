using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BossActionStep
{
    [Tooltip("The movement pattern to execute in this step (can be null).")]
    public BossMovement movement;

    [Tooltip("The bullet pattern to execute in this step (can be null).")]
    public BulletPattern bulletPattern;

    [Tooltip("If true, runs both together. If false, waits for movement to complete before shooting.")]
    public bool executeInParallel;
}

[System.Serializable]
public class BossPhase
{
    [Tooltip("The sequence of action steps (movements & bullet patterns) to run in this phase.")]
    public List<BossActionStep> steps;
}