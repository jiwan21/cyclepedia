using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class BackToMainMenu : MonoBehaviour
{
    void Update()
    {
        if (SocketClientVoice.signalStringVoice.Equals("main menu") || SocketClientVoice.signalStringVoice.Equals(" main menu"))
        {
           // Debug.Log("signalString" + SocketClientVoice.signalStringVoice);
           // SceneManager.UnloadScene("Version1");
            SceneManager.LoadScene("MainMenuScene");
            UnityEngine.Debug.Log("I heard main menu!");
        }
    }
}
