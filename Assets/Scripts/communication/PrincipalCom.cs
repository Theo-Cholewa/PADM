using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class PrincipalCom : MonoBehaviour
{
    [Header("Réseau")]
    public UdpPeer udpPeer;

    [Header("UI - Joueur (flèche)")]
    public RawImage playerIcon;
    public RectTransform playerIconRect => playerIcon != null ? playerIcon.rectTransform : null;

    [Header("Pierres sur le canvas")]
    public StoneUI[] stones;

    [Serializable]
    public struct StoneUI
    {
        public string cubeId;
        public RawImage image;
    }

    [Header("UI Start / Overlay (sur l'île)")]
    public RawImage overlayImage;    // l'image à afficher
    public Button startButton;       // bouton qui apparaît après 3s min si VR ready
    public float minOverlaySecondsIfConnected = 3f;
    
    [Header("Handshake VR")]
    public float helloInterval = 0.5f;
    public int startSendCount = 5;
    public float startSendInterval = 0.15f;
    public int cubeReqSendCount = 5;
    public float cubeReqSendInterval = 0.15f;

    [Header("Mapping (comme ton joueur)")]
    public bool invertUIY = true;
    public float scale = 240f / 14f;
    public Vector2 uiOffset = new Vector2(3f, 6f);
    public float rotationMultiplier = -1f;
    public float rotationOffsetDeg = 0f;

    // --- Thread-safe queue ---
    private readonly Queue<string> messageQueue = new Queue<string>();
    private readonly object queueLock = new object();

    // --- Runtime lookup ---
    private readonly Dictionary<string, RectTransform> stoneById = new Dictionary<string, RectTransform>();

    // --- Connection & state ---
    private bool vrReady = false;
    private bool gameplayStarted = false;

    // --- Cached data ---
    private bool hasCachedPlayer = false;
    private float cachedPlayerX, cachedPlayerZ, cachedPlayerRotY;
    private readonly Dictionary<string, Vector3> cachedCubes = new Dictionary<string, Vector3>();

    // --- Coroutines ---
    private Coroutine connectRoutine;
    private Coroutine overlayRoutine;

    void Awake()
    {
        // Build stone dict
        stoneById.Clear();
        if (stones != null)
        {
            foreach (var s in stones)
            {
                if (string.IsNullOrEmpty(s.cubeId) || s.image == null) continue;
                stoneById[s.cubeId] = s.image.rectTransform;
            }
        }
    }

    void Start()
    {
        if (udpPeer != null)
            udpPeer.OnUdpMessage += ThreadSafeReceive;

        // On tente la connexion VR dès le début (même si l'île n'est pas visible)
        connectRoutine = StartCoroutine(ConnectToVrLoop());
    }

    void OnDestroy()
    {
        if (udpPeer != null)
            udpPeer.OnUdpMessage -= ThreadSafeReceive;
    }

    // L'île/canvas devient visible -> on gère l'overlay/bouton ici
    void OnEnable()
    {
        Debug.Log("[PrincipalCom] OnEnable → Island visible");

        if (overlayRoutine != null)
            StopCoroutine(overlayRoutine);

        overlayRoutine = StartCoroutine(OverlayFlowWhenIslandVisible());
    }


    void OnDisable()
    {
        if (overlayRoutine != null)
        {
            StopCoroutine(overlayRoutine);
            overlayRoutine = null;
        }
    }

    void Update()
    {
        // Process messages on main thread
        while (true)
        {
            string msg = null;
            lock (queueLock)
            {
                if (messageQueue.Count > 0) msg = messageQueue.Dequeue();
            }
            if (msg == null) break;
            ProcessMessage(msg);
        }

        // Si on a démarré, on applique en live les caches (si besoin)
        // (Pas obligatoire ici, on applique au moment de la réception.)
    }

    // ---------------------- NETWORK THREAD SAFE ----------------------

    void ThreadSafeReceive(string msg)
    {
        lock (queueLock)
        {
            messageQueue.Enqueue(msg);
        }
    }

    // ---------------------- CONNECTION LOOP ----------------------

    IEnumerator ConnectToVrLoop()
    {
        // On spam gentiment HELLO jusqu'à recevoir VR_READY
        while (!vrReady)
        {
            if (udpPeer != null)
            {
                udpPeer.SendRaw("HELLO");
                // Debug.Log("[PrincipalCom] Sent: HELLO");
            }
            yield return new WaitForSecondsRealtime(helloInterval);
        }

        // Dès qu'on est connecté, on demande les cubes pour remplir le cache
        yield return StartCoroutine(SendCubeReqBurst());
    }

    IEnumerator SendCubeReqBurst()
    {
        if (udpPeer == null) yield break;

        for (int i = 0; i < cubeReqSendCount; i++)
        {
            udpPeer.SendRaw("CUBE_REQ");
            // Debug.Log($"[PrincipalCom] Sent: CUBE_REQ ({i+1}/{cubeReqSendCount})");
            yield return new WaitForSecondsRealtime(cubeReqSendInterval);
        }
    }

    // ---------------------- OVERLAY FLOW (when island visible) ----------------------

    IEnumerator OverlayFlowWhenIslandVisible()
    {
        Debug.Log("[PrincipalCom] Overlay flow started");

        // 1) L'image est visible
        if (overlayImage != null)
            overlayImage.gameObject.SetActive(true);

        if (startButton != null)
        {
            startButton.gameObject.SetActive(false);
            startButton.interactable = false;
        }

        // 2) Tant que le VR n'est PAS prêt → on garde l'image
        while (!vrReady)
        {
            yield return null;
        }

        Debug.Log("[PrincipalCom] VR ready detected → waiting minimum time");

        // 3) VR prêt → on attend au minimum 3 secondes
        yield return new WaitForSecondsRealtime(minOverlaySecondsIfConnected);

        // 4) Transition UI
        if (overlayImage != null)
            overlayImage.gameObject.SetActive(false);

        if (startButton != null)
        {
            startButton.gameObject.SetActive(true);
            startButton.interactable = true;
        }

        Debug.Log("[PrincipalCom] Overlay hidden, Start button shown");
    }


    // ---------------------- BUTTON START ----------------------

    public void OnStartButtonClicked()
    {
        if (!vrReady || udpPeer == null) return;

        // On lance le jeu: envoi start (fiable) + on redemande les cubes
        StartCoroutine(SendStartBurst());

        // UI: cache overlay + bouton, montre le labyrinthe (si tu as un panel)
        if (overlayImage != null) overlayImage.gameObject.SetActive(false);
        if (startButton != null)
        {
            startButton.interactable = false;
            startButton.gameObject.SetActive(false);
        }

        gameplayStarted = true;

        // Applique immédiatement ce qu'on a en cache
        //ApplyCachedPlayerIfAny();
        ApplyAllCachedCubes();
    }

    IEnumerator SendStartBurst()
    {
        // Envoie "start" plusieurs fois (UDP) + CUBE_REQ pour resync
        for (int i = 0; i < startSendCount; i++)
        {
            udpPeer.SendRaw("start");
            Debug.Log("[PrincipalCom] Sent: start");
            yield return new WaitForSecondsRealtime(startSendInterval);
        }

        // Resync cubes (optionnel mais pratique)
        yield return StartCoroutine(SendCubeReqBurst());
    }

    // ---------------------- MESSAGE PROCESSING ----------------------

    void ProcessMessage(string msg)
    {
        if (msg == "VR_READY")
        {
            vrReady = true;
            Debug.Log("[PrincipalCom] Received: VR_READY");

            // Si l'île est visible, le bouton sera débloqué après le timer (coroutine)
            return;
        }

        if (msg.StartsWith("POS;"))
        {
            Debug.Log("[PrincipalCom] Received player POS");
            ApplyPlayerPosFromMsg(msg);
            //CachePlayerPos(msg); 
            //ApplyCachedPlayerIfAny();

            return;
        }

        if (msg.StartsWith("CUBE_INIT;"))
        {
            CacheCubePos(msg);
            if (gameplayStarted) ApplyAllCachedCubes(); // ou ApplyOneCube seulement
            return;
        }

        if (msg.StartsWith("CUBE_GRABBED;"))
        {
            ApplyCubeGrabbed(msg);
            return;
        }
    }

    // ---------------------- CACHES ----------------------

    void CachePlayerPos(string msg)
    {
        // POS;x;z;rotY
        var parts = msg.Split(';');
        if (parts.Length != 4) return;

        if (!TryParse(parts[1], out float x) ||
            !TryParse(parts[2], out float z) ||
            !TryParse(parts[3], out float rotY))
            return;

        cachedPlayerX = x;
        cachedPlayerZ = z;
        cachedPlayerRotY = rotY;
        hasCachedPlayer = true;

        float sx = cachedPlayerX * scale;
        float sy = cachedPlayerZ * scale;

        float uiX = sx + uiOffset.x;
        float uiY = sy + uiOffset.y;
        if (invertUIY) uiY = -uiY;

        playerIconRect.anchoredPosition = new Vector2(uiX, uiY);

        float uiAngle = cachedPlayerRotY * rotationMultiplier + rotationOffsetDeg;
        Debug.Log(msg);
    }

    void CacheCubePos(string msg)
    {
        // CUBE_INIT;id;x;y;z
        var parts = msg.Split(';');
        if (parts.Length != 4) return;

        string id = parts[1];
        if (!TryParse(parts[2], out float x) ||
            !TryParse(parts[3], out float y))
            return;

        cachedCubes[id] = new Vector3(y, 0, x);
    }

    // ---------------------- APPLY UI ----------------------

    void ApplyCachedPlayerIfAny()
    {
        if (!hasCachedPlayer || playerIconRect == null) return;

        
        //playerIconRect.localEulerAngles = new Vector3(0f, 0f, uiAngle);
    }

    void ApplyAllCachedCubes()
    {
        foreach (var kv in cachedCubes)
        {
            string id = kv.Key;
            Vector3 p = kv.Value;

            if (!stoneById.TryGetValue(id, out RectTransform rt) || rt == null) continue;

            // Comme joueur: XZ plane
            float sx = p.x * scale;
            float sy = p.z * scale;

            float uiX = sx + uiOffset.x;
            float uiY = sy + uiOffset.y;
            if (invertUIY) uiY = -uiY;

            rt.anchoredPosition = new Vector2(uiX, uiY);
            rt.gameObject.SetActive(true);
        }
    }

    void ApplyCubeGrabbed(string msg)
    {
        // CUBE_GRABBED;id
        var parts = msg.Split(';');
        if (parts.Length != 2) return;

        string id = parts[1];

        // Cache: on peut aussi l'enlever du cache si tu veux
        cachedCubes.Remove(id);

        if (stoneById.TryGetValue(id, out RectTransform rt) && rt != null)
        {
            rt.gameObject.SetActive(false);
        }
    }

    void ApplyPlayerPosFromMsg(string msg)
    {
        // Format : POS;x;z;rotY
        if (playerIconRect == null)
        {
            Debug.LogError("[PrincipalCom] playerIconRect est NULL. Assigne 'playerIcon' (RawImage) dans l'inspector.");
            return;
        }

        var parts = msg.Split(';');
        if (parts.Length != 4)
        {
            Debug.LogWarning("[PrincipalCom] POS invalide: " + msg);
            return;
        }

        if (!TryParse(parts[1], out float z) ||
            !TryParse(parts[2], out float x) ||
            !TryParse(parts[3], out float rotY))
        {
            Debug.LogWarning("[PrincipalCom] POS parse failed: " + msg);
            return;
        }

        // Position UI (comme ton mapping)
        float sx = x * scale;
        float sy = z * scale;

        float uiX = sx + uiOffset.x;
        float uiY = sy + uiOffset.y;
        if (invertUIY) uiY = -uiY;

        playerIconRect.anchoredPosition = new Vector2(uiX, uiY);

        // Rotation UI
        float uiAngle = rotY * rotationMultiplier + rotationOffsetDeg;
        playerIconRect.localEulerAngles = new Vector3(0f, 0f, uiAngle);
    }


    bool TryParse(string s, out float v) =>
        float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v);
}