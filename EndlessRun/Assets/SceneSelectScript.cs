using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;


public class SceneSelectScript : MonoBehaviour
{
    void Update(){

        /*switch (this.gameObject.name){
             case "TutorialButton":
                 SceneManager.LoadScene("Tutorial");
                 break;
             case "GameButton":
                 SceneManager.LoadScene("Version1");
                 break;
         }*/
      
        if (SocketClientVoice.signalStringVoice.Equals("tutorial")|| SocketClientVoice.signalStringVoice.Equals(" tutorial"))
        {
            SceneManager.LoadScene("Tutorial");
            //Debug.Log("I heard tutorial!");
        }
        else if (SocketClientVoice.signalStringVoice.Equals("game") || SocketClientVoice.signalStringVoice.Equals(" game"))
        {
            SceneManager.LoadScene("Version1");
            //Debug.Log("I heard game!");
        }

    }

 
}
