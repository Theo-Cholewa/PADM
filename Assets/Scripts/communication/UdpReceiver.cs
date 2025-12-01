using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UdpReceiver : MonoBehaviour
{
    public int listenPort = 8888;
    private UdpClient client;
    private Thread listenThread;
    private bool running;

    void Start()
    {
        client = new UdpClient(listenPort);
        running = true;
        listenThread = new Thread(ListenLoop);
        listenThread.Start();
    }

    void ListenLoop()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (running)
        {
            try
            {
                byte[] data = client.Receive(ref remoteEndPoint);
                string message = Encoding.UTF8.GetString(data);
                Debug.Log($"[UDP] Reçu de {remoteEndPoint}: {message}");
            }
            catch { }
        }
    }

    void OnDestroy()
    {
        running = false;
        client?.Close();
        listenThread?.Abort();
    }
}
