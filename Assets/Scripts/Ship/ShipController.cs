using UnityEngine;

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
            }
            else
            {
                Debug.Log($"{playerName} relève l’ancre ⚓");
            }
        }

        if (anchorDropped) return;

        // --- Gestion de la vitesse avant/arrière ---
        if (Input.GetKey(moveForward))
            currentSpeed += acceleration * Time.deltaTime;
        else
            currentSpeed -= deceleration * Time.deltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);

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
}