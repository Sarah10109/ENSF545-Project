using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentUI : MonoBehaviour
{
    public ExperimentManager experimentManager;
    public Text statusText;

    private void Update()
    {
        if (experimentManager == null || statusText == null)
            return;

        if (!experimentManager.ExperimentRunning)
        {
            statusText.text = "Press S to start (6 Normal, then 6 Heavy)";
            return;
        }

        string cond = experimentManager.CurrentConditionName;
        int trial = experimentManager.CurrentTrialNumber;
        int total = experimentManager.TrialsPerCondition;

        statusText.text = $"Mode: {cond}    Trial: {trial}/{total}";
    }
}
