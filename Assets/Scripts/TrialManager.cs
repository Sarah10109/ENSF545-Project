using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

/// <summary>
/// Manages simple trials: start position, success check, logging to console and CSV.
/// Singleton so other scripts can access it easily.
/// </summary>
public class TrialManager : MonoBehaviour
{
    public static TrialManager Instance { get; private set; }

    [Header("Experiment Info")]
    public string participantId = "P01";
    public string conditionName = "MousePrototype";  // later: "Realistic", "Exaggerated"
    public int totalTrials = 3;

    [Header("Setup")]
    public BoxGrabController box;
    public Transform boxStartPoint;
    public Transform targetPlatform;

    [Header("Logging")]
    public string fileName = "results.csv";

    private int currentTrial = 0;
    private bool trialRunning = false;
    private float trialStartTime;
    private int dropCount = 0;

    // To avoid spamming the path message, we track if we've shown it
    private bool hasShownPath = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ResetBoxToStart();
        Debug.Log("Press and hold left mouse near the box to start trial 1.");
        Debug.Log("Trial results will be logged to CSV after each successful trial.");
    }

    // Called by BoxGrabController when the box is first grabbed
    public void OnBoxGrabbed()
    {
        if (trialRunning) return;
        if (currentTrial >= totalTrials) return;

        // Start timing this trial
        trialRunning = true;
        trialStartTime = Time.time;
        dropCount = 0;

        Debug.Log($"Trial {currentTrial + 1} started.");
    }

    // Called by BoxGrabController when the box is released
    public void OnBoxReleased(Vector3 releasePosition)
    {
        if (!trialRunning) return;

        bool success = IsOverTarget(releasePosition);

        if (success)
        {
            float duration = Time.time - trialStartTime;
            trialRunning = false;
            currentTrial++;

            Debug.Log($"Trial {currentTrial} SUCCESS. Time: {duration:F2}s, Drops: {dropCount}");

            // Log to CSV
            LogTrialToCsv(duration, dropCount);

            ResetBoxToStart();

            if (currentTrial < totalTrials)
            {
                Debug.Log($"Prepare for trial {currentTrial + 1}.");
            }
            else
            {
                Debug.Log("All trials finished.");
            }
        }
        else
        {
            // Missed target: count as a drop, trial continues
            dropCount++;
            Debug.Log($"Box released outside target. Drops so far: {dropCount}");
        }
    }

    bool IsOverTarget(Vector3 pos)
    {
        if (targetPlatform == null) return false;

        // Simple axis-aligned box around target platform
        Vector3 tp = targetPlatform.position;
        Vector3 half = targetPlatform.localScale * 0.5f;

        bool withinX = Mathf.Abs(pos.x - tp.x) <= half.x;
        bool withinZ = Mathf.Abs(pos.z - tp.z) <= half.z;
        bool aboveY  = pos.y >= tp.y;

        return withinX && withinZ && aboveY;
    }

    void ResetBoxToStart()
    {
        if (box != null && boxStartPoint != null)
        {
            box.ResetBox(boxStartPoint.position);
        }
    }

    void LogTrialToCsv(float duration, int drops)
    {
        // Where Unity stores persistent data (depends on OS)
        string folder = Application.persistentDataPath;
        string path = Path.Combine(folder, fileName);

        bool writeHeader = !File.Exists(path);

        StringBuilder sb = new StringBuilder();

        if (writeHeader)
        {
            sb.AppendLine("Participant,Condition,Trial,DurationSeconds,Drops");
        }

        sb.AppendLine($"{participantId},{conditionName},{currentTrial},{duration:F3},{drops}");

        File.AppendAllText(path, sb.ToString());

        if (!hasShownPath)
        {
            Debug.Log($"CSV log written to: {path}");
            hasShownPath = true;
        }
        else
        {
            Debug.Log($"Trial {currentTrial} appended to CSV.");
        }
    }
}
