using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    void Update()
    {
        if (SocketClientVoice.signalStringVoice.Equals("main menu") || SocketClientVoice.signalStringVoice.Equals(" main menu"))
        {
            SceneManager.LoadScene("MainMenuScene");
            UnityEngine.Debug.Log("I heard main menu!");
        }
    }
}
