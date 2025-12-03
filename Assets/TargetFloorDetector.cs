using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFloorDetector : MonoBehaviour
{
    [Tooltip("The LiftableBox script on the movable cube (usually on block_puzzle_c_tinker).")]
    public LiftableBox liftableBox;

    [Tooltip("How long the box must stay on the target floor to count as complete (seconds).")]
    public float holdDuration = 1.0f;

    private bool boxInside = false;
    private float insideTimer = 0f;

    private void Reset()
    {
        if (liftableBox == null)
            liftableBox = FindObjectOfType<LiftableBox>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (liftableBox == null)
            return;

        // Only care during an active trial
        if (!liftableBox.TrialActive || liftableBox.TrialCompleted)
            return;

        LiftableBox lb = other.GetComponentInParent<LiftableBox>();
        if (lb == liftableBox)
        {
            boxInside = true;
            insideTimer = 0f;
            // Debug.Log("[TargetFloor] Box entered target area.");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!boxInside || liftableBox == null)
            return;

        if (!liftableBox.TrialActive || liftableBox.TrialCompleted)
            return;

        LiftableBox lb = other.GetComponentInParent<LiftableBox>();
        if (lb != liftableBox)
            return;

        // Count how long we’ve been inside
        insideTimer += Time.deltaTime;

        if (insideTimer >= holdDuration)
        {
            boxInside = false;       // prevent re-trigger in same trial
            insideTimer = 0f;

            // Complete the trial – ExperimentManager will advance to next one
            // Debug.Log("[TargetFloor] Hold time reached, completing trial.");
            liftableBox.CompleteTrial();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        LiftableBox lb = other.GetComponentInParent<LiftableBox>();
        if (lb == liftableBox)
        {
            // Left the floor before the timer finished – reset
            boxInside = false;
            insideTimer = 0f;
            // Debug.Log("[TargetFloor] Box left target area, timer reset.");
        }
    }
}


