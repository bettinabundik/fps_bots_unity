using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FPSBotsLib;

public class ActionText : MonoBehaviour
{
    public int agentID = 1;
    private Text action;

    void Start()
    {
        action = GetComponent<Text>();
        SetActionText(Action.movefw);
    }

    void Update()
    {
        GameObject background = GameObject.Find("Background");
        MainGame maingame = background.GetComponent<MainGame>();

        if (maingame.Model != null)
        {
            int modelindex = maingame.AgentCounter + (agentID - 1);
            Action nextaction = maingame.Model.UltimateData.Agentdata[modelindex].action;

            SetActionText(nextaction);
        }
    }

    private void SetActionText(Action _action)
    {
        if (_action == Action.movefw)
            action.text = "forward";
        else if (_action == Action.movebw)
            action.text = "backward";
        else if (_action == Action.turnleft)
            action.text = "left";
        else if (_action == Action.turnright)
            action.text = "right";
        else if (_action == Action.item)
            action.text = "item";
        else if (_action == Action.shoot)
            action.text = "shoot";
    }
}
