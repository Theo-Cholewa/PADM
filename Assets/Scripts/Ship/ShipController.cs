using UnityEngine;

public class ShipController : MonoBehaviour
{
    public string playerName = "Red";
    private ShipData data;
    private Rigidbody rb;

    [Header("Déplacement du bateau")]
    public string moveForward = "z";
    public string turnLeft = "q";
    public string turnRight = "d";
    public string anchorKey = "s";

    private bool anchorDropped = false;

    [Header("Statistiques du bateau")]
    public float acceleration = 5f;
    public float maxSpeed = 8f;
    public float deceleration = 2f;
    public float rotationSpeed = 100f;

    private float currentSpeed = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        data = GetComponent<ShipData>();
    }

    void Update()
    {
        // --- Détection des touches ---
        if (Input.GetKeyDown(anchorKey))
        {
            anchorDropped = !anchorDropped;
            if (anchorDropped)
            {
                currentSpeed = 0f;
                rb.velocity = Vector3.zero;
                Debug.Log($"{playerName} pose l’ancre ⚓ (le bateau est immobile)");
            }
            else
            {
                Debug.Log($"{playerName} relève l’ancre ⚓");
            }
        }

        // --- Gestion de la vitesse (accélération/décélération) ---
        if (!anchorDropped)
        {
            if (Input.GetKey(moveForward))
                currentSpeed += acceleration * Time.deltaTime;
            else
            {
                if (currentSpeed > 0)
                    currentSpeed -= deceleration * Time.deltaTime;
                else if (currentSpeed < 0)
                    currentSpeed += deceleration * Time.deltaTime;

                if (Mathf.Abs(currentSpeed) < 0.1f)
                    currentSpeed = 0;
            }
        }

        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
    }

    void FixedUpdate()
    {
        if (anchorDropped) return;

        // --- Déplacement ---
        Vector3 movement = transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        // --- Rotation ---
        if (Input.GetKey(turnLeft))
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, -rotationSpeed * Time.fixedDeltaTime, 0));
        if (Input.GetKey(turnRight))
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, rotationSpeed * Time.fixedDeltaTime, 0));
    }
}