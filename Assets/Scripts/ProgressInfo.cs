using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressInfo : MonoBehaviour
{
    private Text progresstext;
    private bool alreadysetreplay;
    private bool alreadysetfinish;

    void Start()
    {
        progresstext = GetComponent<Text>();
        SetProgressText("");
        alreadysetreplay = false;
        alreadysetfinish = false;
    }

    void Update()
    {
        GameObject background = GameObject.Find("Background");
        MainGame maingame = background.GetComponent<MainGame>();

        if (maingame.StillTraining && !maingame.StillReplaying)
            SetProgressText("Navigation Controller Completed.\nCombat Controller Completed.\nUltimate Training Completed.");

        if (maingame.StillReplaying && !alreadysetreplay)
        {
            SetProgressText(progresstext.text + "\nReplaying Ultimate Training...");
            alreadysetreplay = true;
        }

        if (!maingame.StillTraining && !maingame.StillReplaying && !alreadysetfinish)
        {
            SetProgressText(progresstext.text + "\nReplay Finished.");
            alreadysetfinish = true;
        }
    }

    private void SetProgressText(string text)
    {
        progresstext.text = text;
    }
}
