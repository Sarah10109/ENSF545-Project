using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftableBox : MonoBehaviour
{
    [Header("Condition / mass")]
    public string conditionName = "Normal";
    public float massMultiplier = 1f;

    [Header("Materials")]
    public Material normalMaterial;
    public Material heavyMaterial;

    private Rigidbody rb;
    private MeshRenderer meshRenderer;
    private float baseMass;
    private Material originalMaterial;

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

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
        }

        baseMass = rb.mass;
    }

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

        if (meshRenderer != null)
        {
            if (conditionName == "Heavy" && heavyMaterial != null)
            {
                meshRenderer.material = heavyMaterial;
            }
            else if (normalMaterial != null)
            {
                meshRenderer.material = normalMaterial;
            }
            else if (originalMaterial != null)
            {
                meshRenderer.material = originalMaterial;
            }
        }

        Debug.Log($"[LiftableBox] StartTrial cond={conditionName}, massScale={massMultiplier}");
    }

    public void CompleteTrial(float score = 0.5f)
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
                trialTime,
                score
            );
        }

        Debug.Log($"[LiftableBox] Trial COMPLETE. cond={conditionName}, time={trialTime:F3}s, score={score:F2}");
    }

    public void ResetToBaseline()
    {
        TrialActive = false;
        TrialCompleted = false;
        conditionName = "Normal";
        massMultiplier = 1f;
        rb.mass = baseMass;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        if (meshRenderer != null)
        {
            if (normalMaterial != null)
                meshRenderer.material = normalMaterial;
            else if (originalMaterial != null)
                meshRenderer.material = originalMaterial;
        }
    }
}
