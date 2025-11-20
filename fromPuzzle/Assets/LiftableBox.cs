using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftableBox : MonoBehaviour
{
    [Header("Condition / mass")]
    public string conditionName = "Normal";
    public float massMultiplier = 1f;

    private Rigidbody rb;
    private float baseMass;

    public bool TrialActive    { get; private set; }
    public bool TrialCompleted { get; private set; }

    private float trialStartTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("LiftableBox requires a Rigidbody.");
            enabled = false;
            return;
        }

        baseMass = rb.mass;
    }

    /// <summary>Called by ExperimentManager at the start of each trial.</summary>
    public void StartTrial(string newConditionName, float massScale)
    {
        conditionName = newConditionName;
        massMultiplier = massScale;

        TrialActive = true;
        TrialCompleted = false;
        trialStartTime = Time.time;

        rb.mass = baseMass * massMultiplier;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Debug.Log($"[LiftableBox] StartTrial cond={conditionName}, massScale={massMultiplier}");
    }

    /// <summary>Called by the target floor when the cube has been on it long enough.</summary>
    public void CompleteTrial()
    {
        if (!TrialActive || TrialCompleted)
            return;

        TrialCompleted = true;
        TrialActive = false;

        float trialTime = Time.time - trialStartTime;

        if (TrialLogger.Instance != null)
        {
            TrialLogger.Instance.LogTrial(
                TrialLogger.Instance.participantId,
                conditionName,
                massMultiplier,
                trialTime
            );
        }

        Debug.Log($"[LiftableBox] Trial COMPLETE. cond={conditionName}, time={trialTime:F3}s");
    }
}
