using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TrialLogger : MonoBehaviour
{
    public static TrialLogger Instance { get; private set; }

    [Header("Logging")]
    public string participantId = "P01";
    public string fileName = "haptics_trials.csv";

    private string filePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        filePath = Path.Combine(Application.persistentDataPath, fileName);

        if (!File.Exists(filePath))
        {
            string header = "Timestamp,Participant,Condition,MassMultiplier,LiftTimeSeconds\n";
            File.WriteAllText(filePath, header);
        }

        Debug.Log("[TrialLogger] Logging to: " + filePath);
    }

    public void LogTrial(string participantId, string conditionName, float massMultiplier, float liftTimeSeconds)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string line = string.Format(
            "{0},{1},{2},{3},{4:F3}\n",
            timestamp,
            participantId,
            conditionName,
            massMultiplier.ToString("F2"),
            liftTimeSeconds
        );

        File.AppendAllText(filePath, line);
    }
}
