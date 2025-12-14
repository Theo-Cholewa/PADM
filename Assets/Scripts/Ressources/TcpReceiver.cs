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
    public ShipController redShip;
    public ShipController blueShip;

    [Header("Debug")]
    public bool logMessages = true;

    private TcpListener listener;
    private Thread listenThread;
    private bool running;

    // Dernières valeurs reçues par équipe
    private volatile float latestSteerRed = 0f;
    private volatile float latestSteerBlue = 0f;

    private volatile float latestThrottleRed = 0f;
    private volatile float latestThrottleBlue = 0f;

    // ⚓ Demande de toggle d’ancre par équipe
    private volatile bool anchorToggleRedRequested = false;
    private volatile bool anchorToggleBlueRequested = false;

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

                    if (logMessages)
                        Debug.Log($"[TCP] Reçu: {line}");

                    var parts = line.Split(new[] { ':' }, 2);
                    if (parts.Length != 2)
                    {
                        if (logMessages)
                            Debug.LogWarning($"[TCP] Format invalide: {line}");
                        continue;
                    }

                    var team = parts[0].Trim().ToUpperInvariant(); // "RED" / "BLUE"
                    var payload = parts[1].Trim();

                    bool isBlue = team == "BLUE";
                    bool isRed = team == "RED";

                    if (!isBlue && !isRed)
                    {
                        if (logMessages) Debug.LogWarning($"[TCP] Team inconnue: {team} ({line})");
                        continue;
                    }

                    if (payload.StartsWith("STEER:", StringComparison.OrdinalIgnoreCase))
                    {
                        var valStr = payload.Substring("STEER:".Length);
                        if (float.TryParse(valStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float steer))
                        {
                            steer = Mathf.Clamp(steer, -1f, 1f);
                            if (isBlue) latestSteerBlue = steer;
                            else latestSteerRed = steer;
                        }
                    }
                    else if (payload.StartsWith("THR:", StringComparison.OrdinalIgnoreCase))
                    {
                        var valStr = payload.Substring("THR:".Length);
                        if (float.TryParse(valStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float thr))
                        {
                            thr = Mathf.Clamp01(thr);
                            if (isBlue) latestThrottleBlue = thr;
                            else latestThrottleRed = thr;
                        }
                    }
                    else if (payload.StartsWith("ANCHOR:", StringComparison.OrdinalIgnoreCase))
                    {
                        // On ne toggle que si c'est vraiment "ANCHOR:TOGGLE"
                        if (payload.Equals("ANCHOR:TOGGLE", StringComparison.OrdinalIgnoreCase))
                        {
                            if (isBlue) anchorToggleBlueRequested = true;
                            else anchorToggleRedRequested = true;
                        }
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
        if (redShip != null)
        {
            if (redShip.useNetworkSteering)
                redShip.SetNetworkSteer(latestSteerRed);

            if (redShip.useNetworkThrottle)
                redShip.SetNetworkThrottle(latestThrottleRed);

            if (anchorToggleRedRequested)
            {
                anchorToggleRedRequested = false;
                redShip.ToggleAnchorFromNetwork();
            }
        }

        if (blueShip != null)
        {
            if (blueShip.useNetworkSteering)
                blueShip.SetNetworkSteer(latestSteerBlue);

            if (blueShip.useNetworkThrottle)
                blueShip.SetNetworkThrottle(latestThrottleBlue);

            if (anchorToggleBlueRequested)
            {
                anchorToggleBlueRequested = false;
                blueShip.ToggleAnchorFromNetwork();
            }
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
