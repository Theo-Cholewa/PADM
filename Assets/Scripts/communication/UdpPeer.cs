using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UdpPeer : MonoBehaviour
{
    private static UdpPeer Instance;

    [Header("Identité")]
    public string peerId = "A";

    [Header("Réseau")]
    public string remoteIp = "127.0.0.1";
    public int remotePort = 8888;
    public int listenPort = 8888;

    [Header("Ships Controllers")]
    public ShipController redShipController;
    public ShipController blueShipController;

    private UdpClient udpClient;
    private Thread listenThread;
    private volatile bool running = false;

    public Action<string> OnUdpMessage;

    [Header("Debug")]
    public bool verboseLogs = true;

    [Header("Network timeouts")]
    [Tooltip("Si aucun message WHEEL n'arrive pendant ce délai, on reset steer à 0.")]
    public float wheelTimeout = 0.25f;

    [Tooltip("Si aucun message THROTTLE n'arrive pendant ce délai, on reset throttle à 0.")]
    public float throttleTimeout = 0.25f;

    [Tooltip("Si true: après timeout, on remet useNetworkX=false (retour clavier).")]
    public bool releaseToKeyboardOnTimeout = false;

    private SynchronizationContext unityContext;

    private float lastWheelBlue = -999f;
    private float lastWheelRed = -999f;
    private float lastThrottleBlue = -999f;
    private float lastThrottleRed = -999f;

    private float nextTimeoutCheck = 0f;

    void Awake()
    {
        // ✅ Anti-instances fantômes (prefab instancié, DontDestroyOnLoad, reload scène, etc.)
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[UDP:{peerId}] DUPLICATE UdpPeer detected. Keeping '{Instance.gameObject.name}' (id={Instance.GetInstanceID()}), destroying '{gameObject.name}' (id={GetInstanceID()}).");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        unityContext = SynchronizationContext.Current;

        if (verboseLogs)
            Debug.Log($"[UDP:{peerId}] Awake OK | obj={gameObject.name} id={GetInstanceID()} active={gameObject.activeInHierarchy} enabled={enabled}");
    }

    void Start()
    {
        try
        {
            if (unityContext == null)
                unityContext = SynchronizationContext.Current;

            Debug.Log($"[UDP:{peerId}] Start | listenPort={listenPort} remote={remoteIp}:{remotePort} | obj={gameObject.name} id={GetInstanceID()} active={gameObject.activeInHierarchy} enabled={enabled} timeScale={Time.timeScale}");

            udpClient = new UdpClient(listenPort);
            udpClient.EnableBroadcast = true;

            running = true;

            listenThread = new Thread(ListenLoop) { IsBackground = true };
            listenThread.Start();

            Debug.Log($"[UDP:{peerId}] Écoute OK sur le port {listenPort}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[UDP:{peerId}] Erreur d'initialisation UDP: {e}");
        }
    }

    private void ListenLoop()
    {
        var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (running)
        {
            try
            {
                byte[] data = udpClient.Receive(ref remoteEndPoint);
                string msg = Encoding.UTF8.GetString(data);

                if (verboseLogs)
                    Debug.Log($"[UDP:{peerId}] (THREAD) Reçu <- {remoteEndPoint.Address}:{remoteEndPoint.Port} | {msg}");

                // ✅ bascule vers Main Thread Unity
                unityContext?.Post(_ =>
                {
                    if (!running || this == null) return;

                    if (verboseLogs)
                        Debug.Log($"[UDP:{peerId}] (MAIN) Handling -> {msg} | obj={gameObject.name} active={gameObject.activeInHierarchy} enabled={enabled}");

                    OnUdpMessage?.Invoke(msg);
                    CallShipControllers(msg);
                }, null);
            }
            catch (ObjectDisposedException) { break; }
            catch (SocketException) { if (!running) break; }
            catch (Exception e)
            {
                Debug.LogWarning($"[UDP:{peerId}] Erreur réception: {e}");
            }
        }
    }

    void Update()
    {
        // ✅ check timeouts (unscaled) pour éviter "steer bloqué"
        if (Time.unscaledTime >= nextTimeoutCheck)
        {
            nextTimeoutCheck = Time.unscaledTime + 0.05f;
            ApplyTimeouts();
        }
    }

    private void ApplyTimeouts()
    {
        float now = Time.unscaledTime;

        if (blueShipController != null)
        {
            if (blueShipController.useNetworkSteering && (now - lastWheelBlue) > wheelTimeout)
            {
                if (verboseLogs)
                    Debug.Log($"[UDP:{peerId}] (MAIN) WHEEL timeout BLUE -> steer=0 (last={lastWheelBlue:F2} now={now:F2})");

                blueShipController.SetNetworkSteer(0f);
                if (releaseToKeyboardOnTimeout) blueShipController.useNetworkSteering = false;
                lastWheelBlue = now;
            }

            if (blueShipController.useNetworkThrottle && (now - lastThrottleBlue) > throttleTimeout)
            {
                if (verboseLogs)
                    Debug.Log($"[UDP:{peerId}] (MAIN) THROTTLE timeout BLUE -> thr=0 (last={lastThrottleBlue:F2} now={now:F2})");

                blueShipController.SetNetworkThrottle(0f);
                if (releaseToKeyboardOnTimeout) blueShipController.useNetworkThrottle = false;
                lastThrottleBlue = now;
            }
        }

        if (redShipController != null)
        {
            if (redShipController.useNetworkSteering && (now - lastWheelRed) > wheelTimeout)
            {
                if (verboseLogs)
                    Debug.Log($"[UDP:{peerId}] (MAIN) WHEEL timeout RED -> steer=0 (last={lastWheelRed:F2} now={now:F2})");

                redShipController.SetNetworkSteer(0f);
                if (releaseToKeyboardOnTimeout) redShipController.useNetworkSteering = false;
                lastWheelRed = now;
            }

            if (redShipController.useNetworkThrottle && (now - lastThrottleRed) > throttleTimeout)
            {
                if (verboseLogs)
                    Debug.Log($"[UDP:{peerId}] (MAIN) THROTTLE timeout RED -> thr=0 (last={lastThrottleRed:F2} now={now:F2})");

                redShipController.SetNetworkThrottle(0f);
                if (releaseToKeyboardOnTimeout) redShipController.useNetworkThrottle = false;
                lastThrottleRed = now;
            }
        }
    }

    public void SendRaw(string msg)
    {
        if (udpClient == null)
        {
            Debug.LogWarning($"[UDP:{peerId}] SendRaw ignored: udpClient null");
            return;
        }

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(msg);
            udpClient.Send(data, data.Length, remoteIp, remotePort);
            Debug.Log($"[UDP:{peerId}] Envoyé -> {remoteIp}:{remotePort} | {msg}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[UDP:{peerId}] Erreur envoi: {e}");
        }
    }

    public void Send(string payload) => SendRaw($"{peerId}: {payload}");

    private static bool TryParseFloatAny(string s, out float value)
    {
        s = s.Trim().Replace(',', '.');
        return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private void CallShipControllers(string msg)
    {
        // Supporte:
        // "BLUE:ANCHOR:ON" ou "A: BLUE:ANCHOR:ON"
        // "A: BLUE:THROTTLE:OFF"
        // "A: BLUE:WHEEL:9,6"
        var parts = msg.Split(':');
        if (parts.Length < 3)
        {
            if (verboseLogs) Debug.LogWarning($"[UDP:{peerId}] Parse ignored (parts<3): {msg}");
            return;
        }

        int i = 0;
        string first = parts[0].Trim();

        bool firstIsColor =
            first.Equals("RED", StringComparison.OrdinalIgnoreCase) ||
            first.Equals("BLUE", StringComparison.OrdinalIgnoreCase);

        if (!firstIsColor)
        {
            i = 1;
            if (parts.Length < 4)
            {
                if (verboseLogs) Debug.LogWarning($"[UDP:{peerId}] Parse ignored (prefixed but parts<4): {msg}");
                return;
            }
        }

        string shipColor = parts[i].Trim();
        string commandType = parts[i + 1].Trim();
        string commandValue = parts[i + 2].Trim();

        ShipController target =
            shipColor.Equals("RED", StringComparison.OrdinalIgnoreCase) ? redShipController :
            shipColor.Equals("BLUE", StringComparison.OrdinalIgnoreCase) ? blueShipController :
            null;

        if (verboseLogs)
            Debug.Log($"[UDP:{peerId}] Parsed -> ship={shipColor} type={commandType} value={commandValue} | target={(target != null ? target.gameObject.name : "NULL")}");

        if (target == null) return;

        if (commandType.Equals("ANCHOR", StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log($"[UDP:{peerId}] -> {shipColor} ANCHOR TOGGLE");
            target.ToggleAnchorFromNetwork();
            
            return;
        }

        if (commandType.Equals("THROTTLE", StringComparison.OrdinalIgnoreCase))
        {
            float throttle;
            if (commandValue.Equals("ON", StringComparison.OrdinalIgnoreCase)) throttle = 1f;
            else if (commandValue.Equals("OFF", StringComparison.OrdinalIgnoreCase)) throttle = 0f;
            else if (TryParseFloatAny(commandValue, out var v)) throttle = Mathf.Clamp01(v);
            else return;

            target.useNetworkThrottle = true;
            target.SetNetworkThrottle(throttle);

            float now = Time.unscaledTime;
            if (shipColor.Equals("BLUE", StringComparison.OrdinalIgnoreCase)) lastThrottleBlue = now;
            else lastThrottleRed = now;

            if (verboseLogs)
                Debug.Log($"[UDP:{peerId}] -> {shipColor} THROTTLE={throttle:F2} applied (t={now:F2})");
            return;
        }

        if (commandType.Equals("WHEEL", StringComparison.OrdinalIgnoreCase))
        {
            if (!TryParseFloatAny(commandValue, out var wheelRaw)) return;

            float steer = Mathf.Clamp(wheelRaw / 30f, -1f, 1f);

            target.useNetworkSteering = true;
            target.SetNetworkSteer(steer);

            float now = Time.unscaledTime;
            if (shipColor.Equals("BLUE", StringComparison.OrdinalIgnoreCase)) lastWheelBlue = now;
            else lastWheelRed = now;

            if (verboseLogs)
                Debug.Log($"[UDP:{peerId}] -> {shipColor} WHEEL raw={wheelRaw:F2} steer={steer:F2} applied (t={now:F2})");
            return;
        }
    }

    void OnApplicationQuit() => StopUdp();
    void OnDestroy() => StopUdp();

    private void StopUdp()
    {
        if (!running) return;
        running = false;

        if (verboseLogs)
            Debug.Log($"[UDP:{peerId}] StopUdp called.");

        try { udpClient?.Close(); } catch { }
        udpClient = null;
        listenThread = null;
    }
}