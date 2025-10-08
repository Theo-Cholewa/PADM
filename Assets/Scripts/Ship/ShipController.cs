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

    public float acceleration = 5f;
    public float maxSpeed = 8f;
    public float deceleration = 2f;
    public float rotationSpeed = 100f;

    private float currentSpeed = 0f;
    
    [Header("Actions du bateau")]

    public string addFoodKey = "a";
    public string addWoodKey = "e";
    public string addStoneKey = "r";
    

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        data = GetComponent<ShipData>();
    }

    void FixedUpdate()
    {
        // Accélération
        if (Input.GetKey(moveForward))
        {
            currentSpeed += acceleration * Time.fixedDeltaTime;
        }
        else
        {
            if (currentSpeed > 0)
                currentSpeed -= deceleration * Time.fixedDeltaTime;
            else if (currentSpeed < 0)
                currentSpeed += deceleration * Time.fixedDeltaTime;

            if (Mathf.Abs(currentSpeed) < 0.1f)
                currentSpeed = 0;
        }

        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);

        // Déplacement avec Rigidbody
        Vector3 movement = transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        // Rotation
        if (Input.GetKey(turnLeft))
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, -rotationSpeed * Time.fixedDeltaTime, 0));
        if (Input.GetKey(turnRight))
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, rotationSpeed * Time.fixedDeltaTime, 0));

        // Actions
        if (Input.GetKeyDown(addFoodKey))
        {
            data.AddResource("food", 10);
            Debug.Log($"{playerName} gagne 10 de nourriture → Total nourriture : {data.food}");
        }
        if (Input.GetKeyDown(addWoodKey))
        {
            data.AddResource("wood", 10);
            Debug.Log($"{playerName} gagne 10 de bois → Total bois : {data.wood}");
        }
        if (Input.GetKeyDown(addStoneKey))
        {
            data.AddResource("stone", 10);
            Debug.Log($"{playerName} gagne 10 de pierre → Total pierre : {data.stone}");
        }
    }
}