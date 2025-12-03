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

        string cond = experimentManager.CurrentConditionName;
        int trial = experimentManager.CurrentTrialNumber;
        int total = experimentManager.TrialsPerCondition;

        if (!experimentManager.ExperimentRunning)
        {
            statusText.text = $"Press S to start ({total} Normal, then {total} Heavy)";
            return;
        }

        statusText.text = $"Mode: {cond}    Trial: {trial}/{total}";
    }
}
