using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TcpReceiver : MonoBehaviour
{
    [Header("TCP")]
    public int listenPort = 8888;

    [Header("Bateau controlle")]
    public ShipController shipController;

    private TcpListener listener;
    private Thread listenThread;
    private bool running;

    private float latestSteer = 0f;
    private float latestThrottle = 0f;

    // ⚓ Demande de toggle d’ancre venue du réseau
    private volatile bool anchorToggleRequested = false;

    void Start()
    {
        try
        {
            listener = new TcpListener(IPAddress.Any, listenPort);
            listener.Start();
            running = true;

            listenThread = new Thread(ListenLoop);
            listenThread.IsBackground = true;
            listenThread.Start();

            Debug.Log($"[TCP] Serveur en écoute sur le port {listenPort}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[TCP] Erreur init: {e.Message}");
        }
    }

    private void ListenLoop()
    {
        try
        {
            while (running)
            {
                var client = listener.AcceptTcpClient();
                Debug.Log("[TCP] Client connecté");

                var t = new Thread(() => HandleClient(client));
                t.IsBackground = true;
                t.Start();
            }
        }
        catch (Exception e)
        {
            if (running)
                Debug.LogWarning($"[TCP] Erreur loop: {e.Message}");
        }
    }

    private void HandleClient(TcpClient client)
    {
        using (client)
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream, Encoding.UTF8))
        {
            try
            {
                while (running && client.Connected)
                {
                    var line = reader.ReadLine();
                    if (line == null) break;

                    Debug.Log($"[TCP] Reçu: {line}");

                    if (line.StartsWith("STEER:", StringComparison.OrdinalIgnoreCase))
                    {
                        var valStr = line.Substring("STEER:".Length);
                        if (float.TryParse(valStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float steer))
                        {
                            steer = Mathf.Clamp(steer, -1f, 1f);
                            latestSteer = steer;
                        }
                    }
                    else if (line.StartsWith("THR:", StringComparison.OrdinalIgnoreCase))
                    {
                        var valStr = line.Substring("THR:".Length);
                        if (float.TryParse(valStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float thr))
                        {
                            thr = Mathf.Clamp01(thr);
                            latestThrottle = thr;
                        }
                    }
                    else if (line.StartsWith("ANCHOR:", StringComparison.OrdinalIgnoreCase))
                    {
                        // Pour l’instant : chaque ANCHOR:TOGGLE déclenche un toggle
                        anchorToggleRequested = true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[TCP] Erreur client: {e.Message}");
            }
        }

        Debug.Log("[TCP] Client déconnecté");
    }

    void Update()
    {
        if (shipController == null) return;

        if (shipController.useNetworkSteering)
        {
            shipController.SetNetworkSteer(latestSteer);
        }

        if (shipController.useNetworkThrottle)
        {
            shipController.SetNetworkThrottle(latestThrottle);
        }

        if (anchorToggleRequested)
        {
            anchorToggleRequested = false;
            shipController.ToggleAnchorFromNetwork();
        }
    }

    void OnDestroy()
    {
        StopServer();
    }

    void OnApplicationQuit()
    {
        StopServer();
    }

    private void StopServer()
    {
        if (!running) return;
        running = false;

        try { listener?.Stop(); } catch { }
        listener = null;
        listenThread = null;
    }
}
