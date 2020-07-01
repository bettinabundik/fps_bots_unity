using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitGame : MonoBehaviour
{
    void Start() { }
    void Update() { }

    public void DoExitGame()
    {
        Application.Quit();
        Debug.Log("Exitting game...");
    }
}
