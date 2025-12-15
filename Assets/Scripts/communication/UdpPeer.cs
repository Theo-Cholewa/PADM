using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UdpPeer : MonoBehaviour
{
    [Header("Identité")]
    [Tooltip("Identifiant qui sera préfixé à chaque message envoyé (ex: Phone, VR, Table1...)")]
    public string peerId = "A";

    [Header("Réseau")]
    [Tooltip("Adresse IP de l'autre projet Unity (machine distante, ou 127.0.0.1 en local)")]
    public string remoteIp = "127.0.0.1";

    [Tooltip("Port UDP sur lequel l'autre projet écoute")]
    public int remotePort = 8888;

    [Tooltip("Port UDP sur lequel CE projet écoute")]
    public int listenPort = 8888;

    private UdpClient udpClient;
    private Thread listenThread;
    private bool running = false;

    public System.Action<string> OnUdpMessage;

    void Start()
    {
        try
        {
            // On crée un client UDP bindé sur listenPort pour recevoir
            udpClient = new UdpClient(listenPort);
            running = true;

            listenThread = new Thread(ListenLoop);
            listenThread.IsBackground = true;
            listenThread.Start();

            Debug.Log($"[UDP:{peerId}] Écoute sur le port {listenPort}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[UDP:{peerId}] Erreur d'initialisation: {e.Message}");
        }
    }

    private void ListenLoop()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (running)
        {
            try
            {
                byte[] data = udpClient.Receive(ref remoteEndPoint);
                string msg = Encoding.UTF8.GetString(data);

                // ⚠ On évite d'appeler Unity directement depuis le thread (mais Debug.Log passe en général)
                //Debug.Log($"[UDP:{peerId}] Reçu de {remoteEndPoint.Address}:{remoteEndPoint.Port} -> {msg}");
                OnUdpMessage?.Invoke(msg);

                // Si tu veux parser id + message, tu peux faire :
                // var split = msg.Split(new[] { ':' }, 2);
                // string senderId = split[0];
                // string payload = split.Length > 1 ? split[1].TrimStart() : "";
            }
            catch (ObjectDisposedException)
            {
                // udpClient fermé -> on sort
                break;
            }
            catch (SocketException)
            {
                if (!running) break;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[UDP:{peerId}] Erreur réception: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Envoie un message brut (sans préfixer par l'id).
    /// </summary>
    public void SendRaw(string msg)
    {
        if (udpClient == null) return;

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(msg);
            udpClient.Send(data, data.Length, remoteIp, remotePort);
            Debug.Log($"[UDP:{peerId}] Envoyé -> {remoteIp}:{remotePort} | {msg}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[UDP:{peerId}] Erreur envoi: {e.Message}");
        }
    }

    /// <summary>
    /// Envoie un message en le préfixant par "id: ".
    /// Exemple : "Phone: Hello" si peerId = "Phone".
    /// </summary>
    public void Send(string payload)
    {
        string fullMessage = $"{peerId}: {payload}";
        SendRaw(fullMessage);
    }
    
    void OnApplicationQuit()
    {
        StopUdp();
    }

    void OnDestroy()
    {
        StopUdp();
    }

    private void StopUdp()
    {
        if (!running) return;
        running = false;

        try
        {
            udpClient?.Close();
        }
        catch { }

        // Pas besoin d'Abort, le thread sortira quand Receive lèvera une exception
        listenThread = null;
    }
}
