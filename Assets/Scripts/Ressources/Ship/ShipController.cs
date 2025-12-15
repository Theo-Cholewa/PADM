using System;
using UnityEngine;
using UnityEngine.UI;

public class ShipController : MonoBehaviour
{
    public TeamEnum TeamId = TeamEnum.RED;
    public Team team => Team.Of(TeamId);

    private ShipData data;
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

    [Header("Taille des barres (px)")]
    public float resourceMinSize = 1f;
    public float resourceMaxSize = 100f;

    [Header("Statistiques du bateau")]
    public float acceleration = 2f;
    public float maxSpeed = 3.5f;
    public float deceleration = 1f;

    [Header("Rotation inertielle")]
    public float rotationAcceleration = 20f;
    public float rotationDeceleration = 20f;
    public float maxRotationSpeed = 30f;

    private float currentSpeed = 0f;
    private float currentRotationSpeed = 0f;

    // 🔹 île actuellement accostée
    private Island currentIslandDocked = null;

    private PartyTools.ValueClient<(float,float)> directionClient;


    private RessourceClient.TeamClient ressources;

    void Start()
    {
        directionClient = new(
            Party.current,
            $"direction_{team.id}",
            v => JsonUtility.FromJson<(float,float)>(v)
        );

        ressources = RessourceClient.current.Get(team);

        rb = GetComponent<Rigidbody>();
        data = GetComponent<ShipData>();

        if (data != null)
        {
            data.OnResourcesChanged += UpdateResourceBars;
            UpdateResourceBars();
        }

        if (stopImage != null) stopImage.enabled = false;
        if (woodImage != null) woodImage.enabled = false;
        if (foodImage != null) foodImage.enabled = false;
        if (stoneImage != null) stoneImage.enabled = false;
    }

    void Update()
    {
        var data = ressources.value;
        var volantData = directionClient?.GetAggregate((a,b,c)=>(a.Item1+b.Item1, a.Item2+b.Item2),(0f,0f)) ?? (0f,0f);
        var volantCount = directionClient?.GetValues()?.Count ?? 1;
        if(volantCount==0) volantCount = 1;
        

        // --- Gestion de l’ancre (clavier) ---
        if (Input.GetKeyDown(anchorKey))
        {
            ToggleAnchor();
        }

        // Si l’ancre est posée, le bateau ne bouge plus
        if (anchorDropped) return;

        var speed = data.shipLevel;
        if(speed<=0)speed = 1;

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
        addedSpeed += volantData.Item2/volantCount * acceleration;

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
        addedRotationSpeed -= volantData.Item1/volantCount;

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

        currentRotationSpeed = Mathf.Clamp(currentRotationSpeed, -maxRotationSpeed * speed, maxRotationSpeed * speed);
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
            currentIslandDocked = null;

            foreach (Island island in allIslands)
            {
                float distance = Vector3.Distance(transform.position, island.transform.position);
                if (distance <= detectionRadius)
                {
                    island.SetVisited(true);
                    currentIslandDocked = island;

                        Debug.Log($"⚓ {team} est ancré près de l’île {island.islandID} (dist={distance:F1})");

                        if (island.islandContent != null)
                        {
                            // --- Actions selon la ressource principale ---
                            switch (island.mainResource)
                            {
                                case Island.RessourceType.Food:
                                    // 🐔 Gestion des poulets
                                    ChickenNetJoystick net = island.islandContent.GetComponentInChildren<ChickenNetJoystick>(true);
                                    if (net != null)
                                    {
                                        net.SetLinkedShip(this);
                                        Debug.Log($"🍗 L'île {island.islandID} contient des poulets — filet lié à {team}");
                                    }
                                    else
                                    {
                                        Debug.LogWarning($"⚠ Aucun filet trouvé sur l’île {island.islandID}");
                                    }
                                    break;

                            case Island.RessourceType.Wood:
                                Canvas canvas = island.islandContent.GetComponentInChildren<Canvas>(true);
                                WoodHarvestController wood = null;

                                if (canvas != null)
                                    wood = canvas.GetComponentInChildren<WoodHarvestController>(true);

                                if (wood == null)
                                    wood = island.islandContent.GetComponentInChildren<WoodHarvestController>(true);

                                    if (wood != null)
                                    {
                                        wood.gameObject.SetActive(true);
                                        wood.SetLinkedShip(this); // ✅ lie le bateau ici
                                        Debug.Log($"🌲 L'île {island.islandID} contient du bois — récolte activée pour {team} !");
                                    }
                                    else
                                    {
                                        Debug.LogWarning($"⚠ Aucun contrôleur de bois trouvé sur {island.islandID}");
                                    }
                                    break;

                            case Island.RessourceType.Stone:
                                Debug.Log($"🪨 L'île {island.islandID} contient de la pierre — fonctionnalité à venir !");
                                break;

                            case Island.RessourceType.None:
                            default:
                                Debug.Log($"ℹ️ L'île {island.islandID} ne contient aucune ressource exploitable.");
                                break;
                        }
                    }

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

            if (currentIslandDocked != null)
            {
                if (currentIslandDocked.islandContent != null)
                {
                    switch (currentIslandDocked.mainResource)
                    {
                        case Island.RessourceType.Food:
                            ChickenNetJoystick net = currentIslandDocked.islandContent.GetComponentInChildren<ChickenNetJoystick>(true);
                            if (net != null)
                            {
                                net.SetLinkedShip(null);
                                Debug.Log($"🪢 Filet de l’île {currentIslandDocked.islandID} libéré.");
                            }
                            break;

                        case Island.RessourceType.Wood:
                            WoodHarvestController wood = currentIslandDocked.islandContent.GetComponentInChildren<WoodHarvestController>(true);
                            if (wood != null)
                            {
                                wood.SetLinkedShip(null);
                                wood.gameObject.SetActive(false);
                                Debug.Log($"🌲 Récolte de bois désactivée sur l’île {currentIslandDocked.islandID}");
                            }
                            break;

                        case Island.RessourceType.Stone:
                            Debug.Log($"🪨 Fin de la récolte de pierre sur l’île {currentIslandDocked.islandID}");
                            break;
                    }

                    // 🔹 Remet l’île dans son état initial
                    currentIslandDocked.SetVisited(false);
                    Debug.Log($"🏝️ {team} quitte l’île {currentIslandDocked.islandID}, retour à l’état initial.");
                    currentIslandDocked = null;
                }
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
        UpdateResourceImageHeight(foodImage, data.food);
        UpdateResourceImageHeight(woodImage, data.wood);
        UpdateResourceImageHeight(stoneImage, data.stone);
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
}