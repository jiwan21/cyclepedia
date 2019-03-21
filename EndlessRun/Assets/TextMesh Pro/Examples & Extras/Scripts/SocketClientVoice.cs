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
    public static string turn="";
    public static int turnNumber = 0;
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
        print("Sending to 192.168.43.16 : " + portVoice);
        //print("Sending to 127.0.0.1 : " + portVoice);

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
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("192.168.43.16"), portVoice);
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

    void Update(){
        // Split string on spaces (this will separate all the words).
        if(signalStringVoice != ""){
            string[] words = signalStringVoice.Split(',');

            turn = words[0];
            turnNumber =  Int32.Parse(words[1]);
            Debug.Log("turn: " + turn);
            Debug.Log("number: " + turnNumber);
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