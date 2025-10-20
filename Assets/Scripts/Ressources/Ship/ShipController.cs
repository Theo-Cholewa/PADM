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
    [Tooltip("Image affichée quand le bateau est à l'arrêt")]
    public RawImage stopImage;
    [Tooltip("Image représentant la ressource bois (barre)")]
    public RawImage woodImage;
    [Tooltip("Image représentant la ressource nourriture (barre)")]
    public RawImage foodImage;
    [Tooltip("Image représentant la ressource pierre (barre)")]
    public RawImage stoneImage;

    [Header("Taille des barres (px)")]
    [Tooltip("Largeur minimale (px) d'une image de ressource")]
    public float resourceMinSize = 1f;
    [Tooltip("Largeur maximale (px) d'une image de ressource")]
    public float resourceMaxSize = 100f;

    [Header("Statistiques du bateau")]
    public float acceleration = 2f;
    public float maxSpeed = 3.5f;
    public float deceleration = 1f;

    [Header("Rotation inertielle")]
    public float rotationAcceleration = 20f;    // accélération en °/s²
    public float rotationDeceleration = 20f;    // freinage en °/s²
    public float maxRotationSpeed = 30f;        // vitesse angulaire max en °/s

    private float currentSpeed = 0f;
    private float currentRotationSpeed = 0f;    // °/s

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        data = GetComponent<ShipData>();
        // ensure UI images are hidden at start
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
                currentSpeed = 0f;
                currentRotationSpeed = 0f;
                rb.velocity = Vector3.zero;
                Debug.Log($"{playerName} pose l’ancre ⚓");
                if (stopImage != null)
                    stopImage.enabled = true;
                // show resource images when anchored
                if (woodImage != null) woodImage.enabled = true;
                if (foodImage != null) foodImage.enabled = true;
                if (stoneImage != null) stoneImage.enabled = true;
            }
            else
            {
                Debug.Log($"{playerName} relève l’ancre ⚓");
                if (stopImage != null)
                    stopImage.enabled = false;
                // hide resource images when not anchored
                if (woodImage != null) woodImage.enabled = false;
                if (foodImage != null) foodImage.enabled = false;
                if (stoneImage != null) stoneImage.enabled = false;
            }
        }

        if (anchorDropped) return;

        // --- Gestion de la vitesse avant/arrière ---
        if (Input.GetKey(moveForward))
            currentSpeed += acceleration * Time.deltaTime;
        else
            currentSpeed -= deceleration * Time.deltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);

        // Mise à jour de l'image d'arrêt : visible seulement si la vitesse est nulle
        if (stopImage != null)
            stopImage.enabled = (Mathf.Approximately(currentSpeed, 0f) && anchorDropped);

        // si ancré, mettre à jour la taille des barres de ressources
        if (anchorDropped)
        {
            UpdateResourceImageSize(woodImage, data != null ? data.wood : 0);
            UpdateResourceImageSize(foodImage, data != null ? data.food : 0);
            UpdateResourceImageSize(stoneImage, data != null ? data.stone : 0);
        }

        // --- Gestion de la rotation inertielle ---
        if (Input.GetKey(turnLeft))
            currentRotationSpeed -= rotationAcceleration * Time.deltaTime;
        else if (Input.GetKey(turnRight))
            currentRotationSpeed += rotationAcceleration * Time.deltaTime;
        else
        {
            // Décélération naturelle de la rotation
            if (currentRotationSpeed > 0)
                currentRotationSpeed -= rotationDeceleration * Time.deltaTime;
            else if (currentRotationSpeed < 0)
                currentRotationSpeed += rotationDeceleration * Time.deltaTime;

            // Zone morte
            if (Mathf.Abs(currentRotationSpeed) < 0.5f)
                currentRotationSpeed = 0;
        }

        currentRotationSpeed = Mathf.Clamp(currentRotationSpeed, -maxRotationSpeed, maxRotationSpeed);
    }

    void FixedUpdate()
    {
        if (anchorDropped) return;

        // --- Mouvement avant ---
        Vector3 move = transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        // --- Rotation progressive ---
        if (Mathf.Abs(currentRotationSpeed) > 0.01f)
        {
            Quaternion delta = Quaternion.Euler(0f, currentRotationSpeed * Time.fixedDeltaTime, 0f);
            rb.MoveRotation(rb.rotation * delta);
        }
    }

    // met à jour la taille en pixels de l'image de ressource en fonction de la quantité
    void UpdateResourceImageSize(RawImage image, int amount)
    {
        if (image == null) return;
        // mappe directement 1 unité de ressource = 1 pixel, puis clamp
        float width = Mathf.Clamp((float)amount, resourceMinSize, resourceMaxSize);
        RectTransform rt = image.rectTransform;
        Vector2 size = rt.sizeDelta;
        size.x = width;
        rt.sizeDelta = size;
    }
}