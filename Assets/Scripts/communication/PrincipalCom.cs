using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;


public class PrincipalCom : MonoBehaviour
{
    [Header("Réseau")]
    public UdpPeer udpPeer;

    [Header("UI - Icône à déplacer (joueur)")]
    public RawImage icon;
    public RectTransform iconRect => icon != null ? icon.rectTransform : null;

    [Header("Mapping monde -> UI (pierres)")]
    [Tooltip("Facteur monde->UI si tu veux l’utiliser (sinon on garde ton mapping actuel).")]
    public float worldToUiScale = 240f / 14f;
    public Vector2 worldToUiOffset = new Vector2(3f, 6f); // comme ton uiX+3 et uiY+6
    public bool invertUIY = true;

    [Header("Pierres sur le canvas")]
    public StoneUI[] stones;

    [Serializable]
    public struct StoneUI
    {
        public string cubeId;
        public RawImage image;
    }

    // Dico runtime pour accéder vite aux RectTransform
    private readonly Dictionary<string, RectTransform> stoneById = new Dictionary<string, RectTransform>();

    [Header("Rotation de l'icône")]
    public float rotationMultiplier = -1f;
    public float rotationOffsetDeg = 0f;

    // Buffer thread réseau
    private readonly Queue<string> messageQueue = new Queue<string>();
    private readonly object queueLock = new object();

    void Start()
    {
        // Build dictionary
        stoneById.Clear();
        if (stones != null)
        {
            foreach (var s in stones)
            {
                if (string.IsNullOrEmpty(s.cubeId) || s.image == null) continue;
                stoneById[s.cubeId] = s.image.rectTransform;
            }
        }

        udpPeer.OnUdpMessage += ThreadSafeReceive;
        StartCoroutine(HandshakeReliable());    
    }

    IEnumerator HandshakeReliable()
    {
        yield return null;

        int count = 5;
        float interval = 0.2f;

        for (int i = 0; i < count; i++)
        {
            udpPeer.SendRaw("start");     // pour VrCom (joueur)
            udpPeer.SendRaw("CUBE_REQ");  // pour CubeManager (cubes)
            Debug.Log($"[PrincipalCom] Sent start + CUBE_REQ ({i+1}/{count})");
            yield return new WaitForSeconds(interval);
        }
    }


    void OnDestroy()
    {
        if (udpPeer != null)
            udpPeer.OnUdpMessage -= ThreadSafeReceive;
    }

    void Update()
    {
        // Traitement des messages dans le MAIN THREAD
        while (true)
        {
            string msg = null;

            lock (queueLock)
            {
                if (messageQueue.Count > 0)
                    msg = messageQueue.Dequeue();
            }

            if (msg == null)
                break;

            ProcessMessage(msg);
        }

        // Envoi du start
        if (Keyboard.current?.spaceKey.wasPressedThisFrame ?? false)
            udpPeer.SendRaw("start");
    }

    void ThreadSafeReceive(string msg)
    {
        lock (queueLock)
        {
            messageQueue.Enqueue(msg);
        }
    }

    void ProcessMessage(string msg)
    {
        
        if (msg.StartsWith("POS;"))
        {
            ParseAndApplyPos(msg);
            return;
        }

        if (msg.StartsWith("CUBE_INIT;"))
        {
            Debug. Log("Reçu CUBE_INIT");
            ParseAndApplyCubeInit(msg);
            return;
        }

        if (msg.StartsWith("CUBE_GRABBED;"))
        {
            Debug. Log("Reçu CUBE_GRABBED");
            ParseAndApplyCubeGrabbed(msg);
            return;
        }
    }

    // ------------------- JOUEUR (déjà existant) -------------------

    void ParseAndApplyPos(string msg)
    {
        string[] parts = msg.Split(';');
        if (parts.Length != 4) return;

        if (!TryParse(parts[1], out float y) ||
            !TryParse(parts[2], out float x) ||
            !TryParse(parts[3], out float qw))
            return;

        x *= 240f / 14f;
        y *= 240f / 14f;

        UpdateIcon(new Vector3(x, y, 0f), qw);
    }

    void UpdateIcon(Vector3 worldPos, float yawDeg)
    {
        if (iconRect == null) return;

        float uiX = worldPos.x + 3f;
        float uiY = worldPos.y + 6f;
        if (invertUIY) uiY = -uiY;

        iconRect.anchoredPosition = new Vector2(uiX, uiY);

        float uiAngle = yawDeg * rotationMultiplier + rotationOffsetDeg;
        iconRect.localEulerAngles = new Vector3(0f, 0f, uiAngle);
    }

    // ------------------- PIERRES (nouveau) -------------------

    void ParseAndApplyCubeInit(string msg)
    {
        Debug. Log("ParseAndApplyCubeInit appelé");
        // Format attendu : CUBE_INIT;<id>;<x>;<y>;<z>
        string[] parts = msg.Split(';');
        if (parts.Length != 4) return;

        string id = parts[1];

        if (!TryParse(parts[2], out float worldZ) ||
            !TryParse(parts[3], out float worldX))
            return;

        if (!stoneById.TryGetValue(id, out RectTransform rt) || rt == null)
            return;

        // 👉 Même logique que le joueur :
        // - plan XZ
        // - même scale
        float x = worldX * (240f / 14f);
        float y = -worldZ * (240f / 14f);

        rt.anchoredPosition = new Vector2(x, y);
        Debug.Log($"Placé pierre {id} en UI à ({x}, {y})");

        // sécurité : on s’assure qu’il est visible
        rt.gameObject.SetActive(true);
    }


    void ParseAndApplyCubeGrabbed(string msg)
    {
        // Format attendu : CUBE_GRABBED;<id>
        string[] parts = msg.Split(';');
        if (parts.Length != 2) return;

        string id = parts[1];

        if (stoneById.TryGetValue(id, out RectTransform rt) && rt != null)
        {
            rt.gameObject.SetActive(false); // retire du canvas
        }
    }

    Vector2 WorldToUi(float worldX, float worldY)
    {
        float uiX = worldX * worldToUiScale + worldToUiOffset.x;
        float uiY = worldY * worldToUiScale + worldToUiOffset.y;
        if (invertUIY) uiY = -uiY;
        return new Vector2(uiX, uiY);
    }

    bool TryParse(string s, out float v) =>
        float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v);
}