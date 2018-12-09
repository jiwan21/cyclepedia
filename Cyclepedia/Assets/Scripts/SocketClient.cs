using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class SocketClient : MonoBehaviour {

    // Use this for initialization

    Thread receiveThread;
    UdpClient client;
    public int port;

    //info

    public string lastReceivedUDPPacket = "";
    public string allReceivedUDPPackets = "";

    void Start () {
        init();
    }


    private void init(){
        print ("UPDSend.init()");

        port = 5065;

        print ("Sending to 127.0.0.1 : " + port);

        receiveThread = new Thread (new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start ();

    }

    private void ReceiveData(){
        client = new UdpClient (port);
        while (true) {
            try{
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
                byte[] data = client.Receive(ref anyIP);

                string text = Encoding.UTF8.GetString(data);
                UnityEngine.Debug.Log (">> " + text);
                lastReceivedUDPPacket=text;
                allReceivedUDPPackets=allReceivedUDPPackets+text;

            }catch(Exception e){
                print (e.ToString());
            }
        }
    }

    public string getLatestUDPPacket(){
        allReceivedUDPPackets = "";
        return lastReceivedUDPPacket;
    }
    
    // Update is called once per frame
    void Update () {
        //hero.transform.position = new Vector3(xPos-6.0f,-3,0);
    }

    void OnApplicationQuit(){
        if (receiveThread != null) {
            receiveThread.Abort();
            Debug.Log(receiveThread.IsAlive); //must be false
        }
    }
}