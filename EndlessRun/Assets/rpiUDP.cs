using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class rpiUDP : MonoBehaviour
{
    // Start is called before the first frame update
    Thread thread;
    UdpClient client;
    private int udpPort = 12346;
    public static int bikeSpeed = 0;
    public static float bikeAngle = 0;
    private int[] bikeSpeeds = new int[5];
    private float[] bikeAngles = new float[3];
    private int index = 0;

    void Start()
    {
        thread = new Thread(new ThreadStart(ReceiveData));
        client = new UdpClient(udpPort);
        thread.IsBackground = true;
        thread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ReceiveData()
    {
    	while (true)
    	{
    		IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
    		byte[] receiveBytes = client.Receive(ref RemoteIpEndPoint);
    		string returnData = Encoding.ASCII.GetString(receiveBytes);
            Debug.Log(returnData);
            if(returnData.Length > 1)
            {
                string[] words = returnData.Split(' ');
                //Debug.Log("Delimiter: " + delimit);
                //Debug.Log("Index = " + delimit);

        		//nt pedalSpeed = Int32.Parse(returnData.Substring(0,delimit));
                //int roll = Int32.Parse(returnData.Substring(delimit + 1, returnData.Length - 2));

                int pedalSpeed = Int32.Parse(words[0]);
                float roll = Int32.Parse(words[1])/100.0f;
        		bikeSpeeds[index % 5] = pedalSpeed/3;
        		bikeSpeed = average(bikeSpeeds);
                bikeAngles[index % 3] = roll;
                bikeAngle = average(bikeAngles);

        		Debug.Log("Bike Speed: " + bikeSpeed + "| Roll: " + roll);
                index++;
            }
    	}
    }

    private int average(int[] speeds)
    {
    	int sum = 0;
    	for(int i =0; i < speeds.Length; i++)
    	{
    		sum = sum + speeds[i];
    	}
    	return sum/speeds.Length;
    }

    private float average(float[] speeds)
    {
        float sum = 0;
        for (int i = 0; i < speeds.Length; i++)
        {
            sum = sum + speeds[i];
        }
        return sum / speeds.Length;
    }
}
