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
    [Tooltip("Si activé, la rotation vient du mobile plutôt que du clavier.")]
    public bool useNetworkSteering = false;

    [Tooltip("Si activé, l'accélération vient du mobile plutôt que du clavier.")]
    public bool useNetworkThrottle = false;

    [Tooltip("Entrée réseau normalisée [-1,1] (mise à jour par TcpReceiver).")]
    [Range(-1f, 1f)]
    public float networkSteerInput = 0f;

    [Tooltip("Entrée de throttle réseau [0,1] (0 = coupé, 1 = bouton appuyé).")]
    [Range(0f, 1f)]
    public float networkThrottleInput = 0f;

    public void SetNetworkSteer(float value)
    {
        networkSteerInput = Mathf.Clamp(value, -1f, 1f);
    }

    public void SetNetworkThrottle(float value)
    {
        networkThrottleInput = Mathf.Clamp01(value);
    }

    // 🔹 Appelé par TcpReceiver quand il reçoit ANCHOR:TOGGLE
    public void ToggleAnchorFromNetwork()
    {
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

    [Header("Rotation inertielle")]
    public float rotationAcceleration = 20f;
    public float rotationDeceleration = 20f;
    public float maxRotationSpeed = 30f;

    private float currentSpeed = 0f;
    private float currentRotationSpeed = 0f;
    private float fightTimer = 120f;

    // 🔹 île actuellement accostée
    private Island CurrentDockedIsland = null;

    private PartyTools.ValueClient<(float, float)> directionClient;

    [HideInInspector]
    public RessourceClient.TeamClient ressources;

    void Start()
    {
        directionClient = new(
            Party.current,
            $"direction_{team.id}",
            v => JsonUtility.FromJson<(float, float)>(v)
        );

        ressources = RessourceClient.current.Get(team);

        rb = GetComponent<Rigidbody>();

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
        ressources.onChange.RemoveListener(UpdateResourceBars);

        OnDestroySave();
    }

    void Update()
    {
        var data = ressources.value ?? new();
        var volantData = directionClient?.GetAggregate((a, b, c) => (a.Item1 + b.Item1, a.Item2 + b.Item2), (0f, 0f)) ?? (0f, 0f);
        var volantCount = directionClient?.GetValues()?.Count ?? 1;
        if (volantCount == 0) volantCount = 1;

        // --- Gestion de la baguarre
        fightTimer -= Time.deltaTime;
        if (fightTimer < 0)
        {
            fightImage.enabled = true;
        }

        // --- Gestion de l’ancre (clavier) ---
        if (Input.GetKeyDown(anchorKey))
        {
            ToggleAnchor();
        }

        // Si l’ancre est posée, le bateau ne bouge plus
        if (anchorDropped) return;

        var speed = data.shipLevel;
        if (speed <= 0) speed = 1;

        // --- Mouvement avant/arrière ---
        var addedSpeed = 0f;

        // Network 
        if (useNetworkThrottle)
        {
            if (networkThrottleInput > 0.5f) addedSpeed += acceleration;
        }

        // Contrôle clavier classique
        if (Input.GetKey(moveForward)) addedSpeed += acceleration;

        // Party
        addedSpeed += volantData.Item2 / volantCount * acceleration;

        if (addedSpeed > 0f) currentSpeed += addedSpeed * speed * Time.deltaTime;
        else currentSpeed -= deceleration * speed * Time.deltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed * speed);


        // --- Rotation ---
        var addedRotationSpeed = 0f;

        // Network
        if (useNetworkSteering)
        {
            float steer = networkSteerInput;      // -1 à 1
            addedRotationSpeed += steer;
        }

        // Keyboard
        if (Input.GetKey(turnLeft))
            addedRotationSpeed += -1f;
        else if (Input.GetKey(turnRight))
            addedRotationSpeed += 1f;

        // Party
        addedRotationSpeed -= volantData.Item1 / volantCount;

        if (Math.Abs(addedRotationSpeed) > 0.01f)
        {
            currentRotationSpeed += addedRotationSpeed * Time.deltaTime * speed * rotationAcceleration;
        }
        else
        {
            if (currentRotationSpeed > 0)
                currentRotationSpeed -= rotationDeceleration * Time.deltaTime * speed;
            else if (currentRotationSpeed < 0)
                currentRotationSpeed += rotationDeceleration * Time.deltaTime * speed;

            if (Mathf.Abs(currentRotationSpeed) < 0.5f)
                currentRotationSpeed = 0;
        }

        var maxRotatSpeed = Math.Abs(addedRotationSpeed)*maxRotationSpeed*speed;

        currentRotationSpeed = Mathf.Clamp(currentRotationSpeed, -maxRotatSpeed, maxRotatSpeed);
    }

    // 🔹 Toute la logique ancre regroupée ici
    private void ToggleAnchor()
    {
        anchorDropped = !anchorDropped;

        if (anchorDropped)
        {
            // Pose de l’ancre
            currentSpeed = 0f;
            currentRotationSpeed = 0f;
            rb.velocity = Vector3.zero;
            Debug.Log($"{team} pose l’ancre ⚓");

            if (stopImage != null) stopImage.enabled = true;
            if (woodImage != null) woodImage.enabled = true;
            if (foodImage != null) foodImage.enabled = true;
            if (stoneImage != null) stoneImage.enabled = true;

            // 🔹 Recherche d’île proche
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
            // Lève l’ancre
            Debug.Log($"{team} relève l’ancre ⚓");

            if (stopImage != null) stopImage.enabled = false;
            if (woodImage != null) woodImage.enabled = false;
            if (foodImage != null) foodImage.enabled = false;
            if (stoneImage != null) stoneImage.enabled = false;

            if (CurrentDockedIsland != null)
            {
                Debug.Log($"🏝️ {team} quitte l’île {CurrentDockedIsland.Name}, retour à l’état initial.");
                CurrentDockedIsland.SetDocked(false);
                CurrentDockedIsland.Behaviour?.Undock(this);
                CurrentDockedIsland = null;
            }
        }
    }

    void FixedUpdate()
    {
        if (anchorDropped) return;

        Vector3 move = transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        if (Mathf.Abs(currentRotationSpeed) > 0.01f)
        {
            Quaternion delta = Quaternion.Euler(0f, currentRotationSpeed * Time.fixedDeltaTime, 0f);
            rb.MoveRotation(rb.rotation * delta);
        }
    }

    // 🔹 Met à jour la hauteur des barres selon les quantités actuelles
    void UpdateResourceBars()
    {
        UpdateResourceImageHeight(foodImage, ressources.value?.chicken ?? 0);
        UpdateResourceImageHeight(woodImage, ressources.value?.wood ?? 0);
        UpdateResourceImageHeight(stoneImage, ressources.value?.rock ?? 0);
    }

    // 🔹 Hauteur = 100 à 10 ressources, 0 à 0 ressource
    void UpdateResourceImageHeight(RawImage image, int amount)
    {
        if (image == null) return;

        RectTransform rt = image.rectTransform;
        Vector2 size = rt.sizeDelta;

        // 0 ressource → 0 px ; 10 ressources → resourceMaxSize px
        float t = Mathf.Clamp01(amount / 10f);
        size.y = Mathf.Lerp(0f, resourceMaxSize, t);

        rt.sizeDelta = size;
    }

    // FIGHT //
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

    // DATA SAVING //
    private static Dictionary<Team, SavedShipData> SAVED = new();

    void StartSave()
    {
        if (SAVED.TryGetValue(team, out var data))
        {
            rb.position = data.position;
            rb.rotation = data.rotation;
        }
    }

    void OnDestroySave()
    {
        SAVED.Add(team, new SavedShipData
        {
            position = rb.position,
            rotation = rb.rotation
        });
    }
}