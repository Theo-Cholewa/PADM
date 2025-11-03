using UnityEngine;
using UnityEngine.UI;

public class ShipController : MonoBehaviour
{
    public string playerName = "Red";
    private ShipData data;
    private Rigidbody rb;

    [Header("Touches de contrôle")]
    public string moveForward = "z";
    public string turnLeft = "q";
    public string turnRight = "d";
    public string anchorKey = "s";

    private bool anchorDropped = false;

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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        data = GetComponent<ShipData>();

        if (data != null){
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
        // --- Gestion de l’ancre ---
        if (Input.GetKeyDown(anchorKey))
        {
            anchorDropped = !anchorDropped;

            if (anchorDropped)
            {
                // Pose de l’ancre
                currentSpeed = 0f;
                currentRotationSpeed = 0f;
                rb.velocity = Vector3.zero;
                Debug.Log($"{playerName} pose l’ancre ⚓");

                if (stopImage != null) stopImage.enabled = true;
                if (woodImage != null) woodImage.enabled = true;
                if (foodImage != null) foodImage.enabled = true;
                if (stoneImage != null) stoneImage.enabled = true;

                // 🔹 Recherche d’île proche (hiérarchie actuelle)
                float detectionRadius = 30f;
                Island[] allIslands = FindObjectsOfType<Island>();
                currentIslandDocked = null;

                foreach (Island island in allIslands)
                {
                    float distance = Vector3.Distance(transform.position, island.transform.position);
                    if (distance <= detectionRadius)
                    {
                        island.SetVisited(true);
                        currentIslandDocked = island;

                        Debug.Log($"⚓ {playerName} est ancré près de l’île {island.islandID} (dist={distance:F1})");

                        // 🔹 Cherche le Net dans l'IslandContent
                        if (island.islandContent != null)
                        {
                            ChickenNetJoystick net = island.islandContent.GetComponentInChildren<ChickenNetJoystick>(true);
                            if (net != null)
                            {
                                net.SetLinkedShip(this);
                                Debug.Log($"🪢 Le filet de l’île {island.islandID} est maintenant lié à {playerName}");
                            }
                            else
                            {
                                Debug.LogWarning($"⚠ Aucun filet trouvé sur l’île {island.islandID}");
                            }
                        }

                        break;
                    }
                }
            }
            else
            {
                // Lève l’ancre
                Debug.Log($"{playerName} relève l’ancre ⚓");

                if (stopImage != null) stopImage.enabled = false;
                if (woodImage != null) woodImage.enabled = false;
                if (foodImage != null) foodImage.enabled = false;
                if (stoneImage != null) stoneImage.enabled = false;

                if (currentIslandDocked != null)
                {
                    // 🔹 Délie le filet de l’île actuelle
                    if (currentIslandDocked.islandContent != null)
                    {
                        ChickenNetJoystick net = currentIslandDocked.islandContent.GetComponentInChildren<ChickenNetJoystick>(true);
                        if (net != null)
                        {
                            net.SetLinkedShip(null);
                            Debug.Log($"🪢 Le filet de l’île {currentIslandDocked.islandID} est maintenant libéré.");
                        }
                    }

                    // 🔹 Remet l’île dans son état initial
                    currentIslandDocked.SetVisited(false);
                    Debug.Log($"🏝️ {playerName} quitte l’île {currentIslandDocked.islandID}, retour à l’état initial.");

                    currentIslandDocked = null;
                }
            }
        }

        if (anchorDropped) return;

        // --- Mouvement avant/arrière ---
        if (Input.GetKey(moveForward))
            currentSpeed += acceleration * Time.deltaTime;
        else
            currentSpeed -= deceleration * Time.deltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);

        // --- Rotation inertielle ---
        if (Input.GetKey(turnLeft))
            currentRotationSpeed -= rotationAcceleration * Time.deltaTime;
        else if (Input.GetKey(turnRight))
            currentRotationSpeed += rotationAcceleration * Time.deltaTime;
        else
        {
            if (currentRotationSpeed > 0)
                currentRotationSpeed -= rotationDeceleration * Time.deltaTime;
            else if (currentRotationSpeed < 0)
                currentRotationSpeed += rotationDeceleration * Time.deltaTime;

            if (Mathf.Abs(currentRotationSpeed) < 0.5f)
                currentRotationSpeed = 0;
        }

        currentRotationSpeed = Mathf.Clamp(currentRotationSpeed, -maxRotationSpeed, maxRotationSpeed);
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

        // on mappe 10 → 100px, 0 → 0px
        size.y = Mathf.Clamp((amount / 10f) * 100f, 0f, 100f);
        rt.sizeDelta = size;
    }

}