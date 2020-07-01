using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public int agentID = 1;
    private Image healthbarimage;

    void Start()
    {
        healthbarimage = GetComponent<Image>();
        SetHealthBarValue(100);
    }

    void Update()
    {
        GameObject background = GameObject.Find("Background");
        MainGame maingame = background.GetComponent<MainGame>();

        if (maingame.Model != null)
        {
            int modelindex = maingame.AgentCounter + (agentID - 1);
            int newhealth = maingame.Model.UltimateData.Agentdata[modelindex].health;

            SetHealthBarValue(newhealth);
        }
    }

    private void SetHealthBarValue(int newhealth)
    {
        if (newhealth >= 0 && newhealth <= 100)
        {
            // Adjust size
            healthbarimage.fillAmount = (float)newhealth / 100;

            // Adjust color
            // Health between 51 and 100
            if (newhealth > 50)
                healthbarimage.color = Color.green;
            // Health between 50 and 21
            else if (newhealth <= 50 && newhealth > 20)
                healthbarimage.color = Color.yellow;
            // Health between 20 and 0
            else
                healthbarimage.color = Color.red;
        }
    }
}
