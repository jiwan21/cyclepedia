using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UDPManager : MonoBehaviour
{
    static UdpClient udp;
    Thread thread;

    void Start()
    {
        udp = new UdpClient(12345);
        thread = new Thread(new ThreadStart(ThreadMethod));
        thread.Start();
    }

    void Update()
    {
    }

    private void ThreadMethod()
    {
        while (true)
        {
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            byte[] receiveBytes = udp.Receive(ref RemoteIpEndPoint);
            string returnData = Encoding.ASCII.GetString(receiveBytes);
            Debug.Log(returnData);
        }
    }
}
