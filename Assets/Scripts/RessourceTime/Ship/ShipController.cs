using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class SavedShipData
{
    public Vector3 position;
    public Quaternion rotation;
}

public class ShipController : MonoBehaviour
{
    public TeamEnum TeamId = TeamEnum.RED;
    public Team team => Team.Of(TeamId);

    private Rigidbody rb;

    [Header("Touches de contrôle")]
    public string moveForward = "z";
    public string turnLeft = "q";
    public string turnRight = "d";
    public string anchorKey = "s";

    private bool anchorDropped = false;

    [Header("Contrôle réseau")]
    [Tooltip("Si true, le steer réseau peut être utilisé (si aucun input clavier prioritaire).")]
    public bool useNetworkSteering = false;

    [Tooltip("Si true, le throttle réseau peut être utilisé (si aucun input clavier prioritaire).")]
    public bool useNetworkThrottle = false;

    [Range(-1f, 1f)]
    public float networkSteerInput = 0f;

    [Range(0f, 1f)]
    public float networkThrottleInput = 0f;

    [Header("Priorités d'input")]
    [Tooltip("Si activé, le clavier override le réseau/legacy quand une touche est pressée.")]
    public bool keyboardOverridesNetwork = true;

    [Header("Debug")]
    public bool verboseLogs = true;
    private float nextDebugTime = 0f;

    public void SetNetworkSteer(float value)
    {
        networkSteerInput = Mathf.Clamp(value, -1f, 1f);
        if (verboseLogs) Debug.Log($"[SHIP:{team}] SetNetworkSteer -> {networkSteerInput:F2}");
    }

    public void SetNetworkThrottle(float value)
    {
        networkThrottleInput = Mathf.Clamp01(value);
        if (verboseLogs) Debug.Log($"[SHIP:{team}] SetNetworkThrottle -> {networkThrottleInput:F2}");
    }

    public void ToggleAnchorFromNetwork()
    {
        if (verboseLogs) Debug.Log($"[SHIP:{team}] ToggleAnchorFromNetwork()");
        ToggleAnchor();
    }

    [Header("UI")]
    public RawImage stopImage;
    public RawImage woodImage;
    public RawImage foodImage;
    public RawImage stoneImage;
    public RawImage fightImage;

    [Header("Taille des barres (px)")]
    public float resourceMinSize = 1f;
    public float resourceMaxSize = 100f;

    [Header("Statistiques du bateau")]
    public float acceleration = 2f;
    public float maxSpeed = 3.5f;
    public float deceleration = 1f;
    public float TimeBeforeFight = 120f;

    [Header("Rotation")]
    [Tooltip("Vitesse max de rotation (deg/s).")]
    public float maxRotationSpeed = 30f;

    [Tooltip("Temps de lissage vers la rotation cible (0.05–0.2 bien).")]
    public float rotationSmoothing = 0.12f;

    private float currentSpeed = 0f;
    private float currentRotationSpeed = 0f; // deg/s
    private float fightTimer = 120f;

    private Island CurrentDockedIsland = null;

    [HideInInspector]
    public RessourceClient.TeamClient ressources;

    private static Dictionary<Team, SavedShipData> SAVED = new();

    [Header("Legacy wheel (directionClient)")]
    [Tooltip("Objet optionnel (ancien système). Si présent et compatible, il fournit steer/throttle agrégés. Sinon fallback réseau/clavier.")]
    public UnityEngine.Object directionClient;

    void Start()
    {
        ressources = RessourceClient.current.Get(team);
        rb = GetComponent<Rigidbody>();

        if (verboseLogs)
        {
            Debug.Log($"[SHIP:{team}] Start | obj={gameObject.name} rb={(rb != null ? "OK" : "NULL")} " +
                      $"kinematic={(rb != null && rb.isKinematic)} active={gameObject.activeInHierarchy} enabled={enabled}");
        }

        ressources.onChange.AddListener(UpdateResourceBars);
        UpdateResourceBars();

        if (stopImage != null) stopImage.enabled = false;
        if (woodImage != null) woodImage.enabled = false;
        if (foodImage != null) foodImage.enabled = false;
        if (stoneImage != null) stoneImage.enabled = false;
        if (fightImage != null) fightImage.enabled = false;

        fightTimer = TimeBeforeFight;

        StartSave();
    }

    void OnDestroy()
    {
        if (ressources != null)
            ressources.onChange.RemoveListener(UpdateResourceBars);

        OnDestroySave();
    }

    void Update()
    {
        var data = ressources.value ?? new();
        float shipLevel = Mathf.Max(1, data.shipLevel);

        // 🔁 Legacy directionClient (si présent et compatible)
        bool hasLegacy = TryGetDirectionClientInputs(out float legacySteer, out float legacyThrottle, out int legacyCount);

        if (verboseLogs && Time.time >= nextDebugTime)
        {
            nextDebugTime = Time.time + 1f;
            Debug.Log($"[SHIP:{team}] state anchor={anchorDropped} speed={currentSpeed:F2} rot={currentRotationSpeed:F2} " +
                      $"useNetThr={useNetworkThrottle} thr={networkThrottleInput:F2} useNetSteer={useNetworkSteering} steer={networkSteerInput:F2} " +
                      $"legacy={(hasLegacy ? $"ON count={legacyCount} steer={legacySteer:F2} thr={legacyThrottle:F2}" : "OFF")}");
        }

        // Fight
        fightTimer -= Time.deltaTime;
        if (fightTimer < 0 && fightImage != null) fightImage.enabled = true;

        // Anchor clavier
        if (Input.GetKeyDown(anchorKey))
        {
            if (verboseLogs) Debug.Log($"[SHIP:{team}] Anchor key pressed ({anchorKey})");
            ToggleAnchor();
        }

        if (anchorDropped) return;

        // =========================
        // INPUT SELECTION
        // =========================
        bool kbForward = Input.GetKey(moveForward);
        bool kbLeft = Input.GetKey(turnLeft);
        bool kbRight = Input.GetKey(turnRight);

        bool kbHasThrottle = kbForward;
        bool kbHasSteer = kbLeft || kbRight;

        // ========== THROTTLE ==========
        float throttle = 0f;

        if (keyboardOverridesNetwork && kbHasThrottle)
        {
            throttle = 1f;
        }
        else if (useNetworkThrottle)
        {
            throttle = networkThrottleInput;
        }
        else if (hasLegacy)
        {
            throttle = legacyThrottle;
        }
        else if (kbHasThrottle)
        {
            throttle = 1f;
        }

        currentSpeed += throttle * acceleration * shipLevel * Time.deltaTime;
        currentSpeed -= deceleration * shipLevel * Time.deltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed * shipLevel);

        // ========== STEER ==========
        float steer = 0f;

        if (keyboardOverridesNetwork && kbHasSteer)
        {
            if (kbLeft) steer -= 1f;
            if (kbRight) steer += 1f;
        }
        else if (useNetworkSteering)
        {
            steer = networkSteerInput;
        }
        else if (hasLegacy)
        {
            steer = legacySteer;
        }
        else
        {
            if (kbLeft) steer -= 1f;
            if (kbRight) steer += 1f;
        }

        // Rotation pilotable : vitesse cible (deg/s)
        float targetRotSpeed = steer * maxRotationSpeed * shipLevel;

        // lissage
        float smooth = Mathf.Max(0.001f, rotationSmoothing);
        currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, targetRotSpeed, Time.deltaTime / smooth);
    }

    void FixedUpdate()
    {
        if (anchorDropped || rb == null) return;

        Vector3 move = transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        if (Mathf.Abs(currentRotationSpeed) > 0.01f)
        {
            Quaternion delta = Quaternion.Euler(0f, currentRotationSpeed * Time.fixedDeltaTime, 0f);
            rb.MoveRotation(rb.rotation * delta);
        }
    }

    private void ToggleAnchor()
    {
        anchorDropped = !anchorDropped;

        if (verboseLogs)
            Debug.Log($"[SHIP:{team}] ToggleAnchor -> anchorDropped={anchorDropped}");

        if (anchorDropped)
        {
            currentSpeed = 0f;
            currentRotationSpeed = 0f;
            if (rb != null) rb.velocity = Vector3.zero;

            Debug.Log($"{team} pose l’ancre ⚓");

            if (stopImage != null) stopImage.enabled = true;
            if (woodImage != null) woodImage.enabled = true;
            if (foodImage != null) foodImage.enabled = true;
            if (stoneImage != null) stoneImage.enabled = true;

            float detectionRadius = 20f;
            Island[] allIslands = FindObjectsOfType<Island>();
            CurrentDockedIsland = null;

            foreach (Island island in allIslands)
            {
                float distance = Vector3.Distance(transform.position, island.transform.position);
                if (distance <= detectionRadius && !island.IsDocked)
                {
                    island.SetDocked(true);
                    CurrentDockedIsland = island;
                    island.Behaviour?.Dock(this);
                    Debug.Log($"⚓ {team} est ancré près de l’île {island.Name} (dist={distance:F1})");
                    break;
                }
            }
        }
        else
        {
            Debug.Log($"{team} relève l’ancre ⚓");

            if (stopImage != null) stopImage.enabled = false;
            if (woodImage != null) woodImage.enabled = false;
            if (foodImage != null) foodImage.enabled = false;
            if (stoneImage != null) stoneImage.enabled = false;

            if (CurrentDockedIsland != null)
            {
                var leftName = CurrentDockedIsland.Name;

                CurrentDockedIsland.SetDocked(false);
                CurrentDockedIsland.Behaviour?.Undock(this);
                CurrentDockedIsland = null;

                Debug.Log($"🏝️ {team} quitte l’île {leftName}, retour à l’état initial.");
            }
        }
    }

    void UpdateResourceBars()
    {
        UpdateResourceImageHeight(foodImage, ressources.value?.chicken ?? 0);
        UpdateResourceImageHeight(woodImage, ressources.value?.wood ?? 0);
        UpdateResourceImageHeight(stoneImage, ressources.value?.rock ?? 0);
    }

    void UpdateResourceImageHeight(RawImage image, int amount)
    {
        if (image == null) return;

        RectTransform rt = image.rectTransform;
        Vector2 size = rt.sizeDelta;

        float t = Mathf.Clamp01(amount / 10f);
        size.y = Mathf.Lerp(0f, resourceMaxSize, t);

        rt.sizeDelta = size;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<ShipController>(out var otherShip))
        {
            if (otherShip.fightTimer < 0f && fightTimer < 0f)
            {
                ressources.AskForFight();
            }
        }
    }

    void StartSave()
    {
        if (SAVED.TryGetValue(team, out var data))
        {
            if (rb != null)
            {
                rb.position = data.position;
                rb.rotation = data.rotation;
            }
        }
    }

    void OnDestroySave()
    {
        if (rb == null) return;

        SAVED[team] = new SavedShipData
        {
            position = rb.position,
            rotation = rb.rotation
        };
    }

    // =========================================================
    // Legacy directionClient compatibility layer (safe)
    // =========================================================

    private bool TryGetDirectionClientInputs(out float steerAvg, out float throttleAvg, out int count)
    {
        steerAvg = 0f;
        throttleAvg = 0f;
        count = 0;

        if (directionClient == null) return false;

        try
        {
            // directionClient.GetValues()?.Count
            var values = InvokeMethod(directionClient, "GetValues");
            if (values is System.Collections.ICollection coll)
                count = coll.Count;
            else if (values is System.Collections.IEnumerable enumerable)
            {
                int c = 0;
                foreach (var _ in enumerable) c++;
                count = c;
            }

            if (count <= 0) count = 1;

            // Try to read aggregate without passing lambdas (safe / reflection-only).
            object agg =
                InvokeMethod(directionClient, "GetAggregate") ??
                GetMemberValue(directionClient, "Aggregate") ??
                GetMemberValue(directionClient, "Current") ??
                GetMemberValue(directionClient, "Value");

            if (agg != null)
            {
                if (TryReadTuple2(agg, out float a, out float b))
                {
                    steerAvg = Mathf.Clamp(a / count, -1f, 1f);
                    throttleAvg = Mathf.Clamp01(b / count);
                    return true;
                }
            }

            return false;
        }
        catch (Exception e)
        {
            if (verboseLogs)
                Debug.LogWarning($"[SHIP:{team}] directionClient incompatible/failed: {e.Message}");
            return false;
        }
    }

    private static object InvokeMethod(UnityEngine.Object obj, string methodName)
    {
        if (obj == null) return null;
        var t = obj.GetType();
        var m = t.GetMethod(methodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        if (m == null) return null;

        // For safety: only call parameterless methods.
        if (m.GetParameters().Length != 0) return null;

        return m.Invoke(obj, null);
    }

    private static object GetMemberValue(UnityEngine.Object obj, string memberName)
    {
        if (obj == null) return null;
        var t = obj.GetType();

        var p = t.GetProperty(memberName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        if (p != null && p.GetIndexParameters().Length == 0) return p.GetValue(obj);

        var f = t.GetField(memberName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        if (f != null) return f.GetValue(obj);

        return null;
    }

    private static bool TryReadTuple2(object tuple, out float item1, out float item2)
    {
        item1 = 0f;
        item2 = 0f;
        if (tuple == null) return false;

        var t = tuple.GetType();

        // fields Item1/Item2
        var f1 = t.GetField("Item1");
        var f2 = t.GetField("Item2");
        if (f1 != null && f2 != null)
        {
            object a = f1.GetValue(tuple);
            object b = f2.GetValue(tuple);

            if (a is float af && b is float bf)
            {
                item1 = af;
                item2 = bf;
                return true;
            }
        }

        // properties Item1/Item2
        var p1 = t.GetProperty("Item1");
        var p2 = t.GetProperty("Item2");
        if (p1 != null && p2 != null)
        {
            object a = p1.GetValue(tuple);
            object b = p2.GetValue(tuple);

            if (a is float af && b is float bf)
            {
                item1 = af;
                item2 = bf;
                return true;
            }
        }

        return false;
    }
}
