using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;


public class SocketClientVoice : MonoBehaviour
{
    Thread receiveThreadVoice;
    UdpClient udpClientVoice;
    public int portVoice;
    public string textVoice;

    //info
    public static string signalStringVoice="";
    public string lastReceivedUDPPacketVoice = "";
    public string allReceivedUDPPacketsVoice = "";

    void Start()
    {
        init();
        /*BackButton.flagBackButton = false;
        SceneSelectScript.flagSceneSelect = false;
        BackToMainMenu.flagBackToMainMen = false;*/
    }


    private void init()
    {
        print("UPDSend.init()");

        portVoice = 5300;
        //print("Sending to 127.0.0.1 : " + portVoice);
        //print("Sending to 131.179.1.238 : " + port);
        print("Sending to 127.0.0.1 : " + portVoice);

        receiveThreadVoice = new Thread(new ThreadStart(ReceiveData));
        receiveThreadVoice.IsBackground = true;
        receiveThreadVoice.Start();
    }

    public void ReceiveData()
    {
        udpClientVoice = new UdpClient(portVoice);

        while (true)
        {
            try
            {
                //IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), portVoice);
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("192.168.0.40"), portVoice);
                byte[] data = udpClientVoice.Receive(ref anyIP);

                textVoice = Encoding.UTF8.GetString(data);
                UnityEngine.Debug.Log(textVoice);
                lastReceivedUDPPacketVoice = textVoice;
                signalStringVoice = textVoice;
                allReceivedUDPPacketsVoice = allReceivedUDPPacketsVoice + textVoice;
            }
            catch (Exception e)
            {
                print(e.ToString());
            }

        }
    }

   static void ResetSignalStringVoice()
    {
        signalStringVoice = "";
    }

    void stopThread()
    {
        if (receiveThreadVoice.IsAlive)
        {
            receiveThreadVoice.Abort();
        }
        udpClientVoice.Close();

    }
}