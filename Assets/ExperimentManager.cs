using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    [Header("Scene references")]
    public BlockPuzzleController blockPuzzleController;   // resets the cube
    public LiftableBox liftableBox;                       // the same cube

    [Header("Mass settings")]
    public float normalMassScale = 1.0f;
    public float heavyMassScale  = 2.0f;

    [Header("Trials")]
    public int trialsPerCondition = 6;

    [Header("Environment references")]
    public TargetFloorDetector targetFloorDetector;

    [Header("Auto fail / restart")]
    [Tooltip("Automatically restart the current trial if the cube falls below this Y position (set to a large negative value to disable).")]
    public float fallResetHeight = -5f;

    [Tooltip("Minimum time (seconds) between auto-restarts, to avoid multiple triggers while the cube is still below the threshold).")]
    public float autoRestartCooldown = 1.0f;

    private enum Phase { None, Normal, Heavy }
    private Phase currentPhase = Phase.None;

    private int currentTrialIndex = 0;   // 0-based
    private bool experimentRunning = false;
    private float lastAutoRestartTime = -999f;

    // ---- Public info for UI ----
    public string CurrentConditionName { get; private set; } = "";
    public int CurrentTrialNumber    => currentTrialIndex + 1;   // 1-based for display
    public int TrialsPerCondition    => trialsPerCondition;
    public bool ExperimentRunning    => experimentRunning;

    private void Start()
    {
        if (liftableBox == null)
            Debug.LogError("[ExperimentManager] LiftableBox reference not set.");
        if (blockPuzzleController == null)
            Debug.LogWarning("[ExperimentManager] BlockPuzzleController not set.");
    }

    private void Update()
    {
        // Start full experiment (6 Normal then 6 Heavy)
        if (Input.GetKeyDown(KeyCode.S) && !experimentRunning)
        {
            StartFullExperiment();
        }

        // Check for trial completion and advance
        if (experimentRunning && liftableBox != null)
        {
            // When LiftableBox marks the trial as complete, start the next one
            if (!liftableBox.TrialActive && liftableBox.TrialCompleted)
            {
                OnTrialFinished();
            }
            else if (liftableBox.TrialActive)
            {
                CheckForAutoRestart();
            }
        }
    }

    private void StartFullExperiment()
    {
        ResetEnvironmentToBaseline(true);
        experimentRunning = true;
        StartCondition(Phase.Normal);
    }

    private void StartCondition(Phase phase)
    {
        currentPhase = phase;
        currentTrialIndex = 0;

        CurrentConditionName = (phase == Phase.Normal) ? "Normal" : "Heavy";

        Debug.Log("[ExperimentManager] Starting condition: " + CurrentConditionName);
        StartNewTrial();
    }

    private void StartNewTrial()
    {
        if (blockPuzzleController != null)
            blockPuzzleController.ResetBlocks();   // sends cube back to start position

        float massScale = (currentPhase == Phase.Heavy) ? heavyMassScale : normalMassScale;
        string condName = (currentPhase == Phase.Heavy) ? "Heavy" : "Normal";

        liftableBox.StartTrial(condName, massScale);

        Debug.Log($"[ExperimentManager] Trial {CurrentTrialNumber}/{trialsPerCondition} in {condName} started.");
    }

    private void CheckForAutoRestart()
    {
        if (fallResetHeight >= 9000f) // sentinel for disabled
            return;

        if (liftableBox == null)
            return;

        if (liftableBox.transform.position.y > fallResetHeight)
            return;

        if (Time.time - lastAutoRestartTime < autoRestartCooldown)
            return;

        lastAutoRestartTime = Time.time;
        Debug.LogWarning("[ExperimentManager] Trial auto-restarted because the cube fell below the threshold.");
        RestartCurrentTrial();
    }

    private void RestartCurrentTrial()
    {
        if (blockPuzzleController != null)
            blockPuzzleController.ResetBlocks();

        float massScale = (currentPhase == Phase.Heavy) ? heavyMassScale : normalMassScale;
        string condName = (currentPhase == Phase.Heavy) ? "Heavy" : "Normal";

        liftableBox.StartTrial(condName, massScale);
    }

    private void OnTrialFinished()
    {
        currentTrialIndex++;

        Debug.Log($"[ExperimentManager] Trial {currentTrialIndex} in {CurrentConditionName} finished.");

        if (currentTrialIndex < trialsPerCondition)
        {
            // More trials in this phase
            StartNewTrial();
        }
        else
        {
            // Switch phase or finish
            if (currentPhase == Phase.Normal)
            {
                Debug.Log("[ExperimentManager] Normal condition complete. Moving to Heavy.");
                StartCondition(Phase.Heavy);
            }
            else
            {
                Debug.Log("[ExperimentManager] Heavy condition complete. Experiment finished.");
                experimentRunning = false;
                currentPhase = Phase.None;
                CurrentConditionName = "";
                ResetEnvironmentToBaseline(true);
            }
        }
    }

    private void ResetEnvironmentToBaseline(bool resetTargetPosition)
    {
        if (blockPuzzleController != null)
            blockPuzzleController.ResetBlocks();

        if (liftableBox != null)
            liftableBox.ResetToBaseline();

        if (resetTargetPosition && targetFloorDetector != null)
            targetFloorDetector.ResetTargetToStart();
    }
}
