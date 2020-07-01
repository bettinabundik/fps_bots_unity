using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsText : MonoBehaviour
{
    private Text stats;

    void Start()
    {
        stats = GetComponent<Text>();
        SetStats(0, 0, 0, 0);
    }

    void Update()
    {
        GameObject background = GameObject.Find("Background");
        MainGame maingame = background.GetComponent<MainGame>();

        SetStats(maingame.ItemCount, maingame.HitCount, maingame.MissCount, maingame.KillCount);
    }

    private void SetStats(int items, int hits, int misses, int kd)
    {
        stats.text = items + "\n" + hits + " / " + misses + "\n" + kd;
    }
}
