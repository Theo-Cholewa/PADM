using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;

public class Battle : MonoBehaviour
{

    UdpClient client;
    IPEndPoint ip;

    void Start()
    {
        client = new UdpClient(8888);
        ip = new IPEndPoint(IPAddress.Any, 0);
        client.Client.ReceiveTimeout = 100;
    }

    void FixedUpdate()
    {
        var bytes = client.Receive(ref ip);
        Debug.Log("Ok");
    }
}
