using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IterationText : MonoBehaviour
{
    private Text iteration;

    void Start()
    {
        iteration = GetComponent<Text>();
        SetIterationText(1);
    }

    // Update is called once per frame
    void Update()
    {
        GameObject background = GameObject.Find("Background");
        MainGame maingame = background.GetComponent<MainGame>();

        SetIterationText(maingame.IterationCounter + 1);
    }

    private void SetIterationText(int i)
    {
        iteration.text = "Iteration " + i;
    }
}
