using UnityEngine;

public class ShipController : MonoBehaviour
{
    [Header("Infos Joueur")]
    public string playerName = "Red";

    [Header("Touches de contrôle (configurables en Inspector)")]
    public string moveForward = "z";
    public string turnLeft = "q";
    public string turnRight = "d";

    [Header("Paramètres de mouvement")]
    public float acceleration = 5f;      // Vitesse à laquelle le bateau accélère
    public float maxSpeed = 8f;          // Vitesse maximale
    public float deceleration = 2f;      // Ralentissement quand aucune touche n'est pressée
    public float rotationSpeed = 100f;   // Vitesse de rotation

    private float currentSpeed = 0f;     // Vitesse actuelle du bateau

    void Update()
    {
        // Gestion de l'accélération
        if (Input.GetKey(moveForward))
        {
            currentSpeed += acceleration * Time.deltaTime;
        }
        else
        {
            // Si aucune touche n'est appuyée, on ralentit progressivement
            if (currentSpeed > 0)
                currentSpeed -= deceleration * Time.deltaTime;
            else if (currentSpeed < 0)
                currentSpeed += deceleration * Time.deltaTime;

            // On évite les très petites valeurs
            if (Mathf.Abs(currentSpeed) < 0.1f)
                currentSpeed = 0;
        }

        // Clamp pour éviter de dépasser la vitesse max
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);

        // Mouvement
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime, Space.Self);

        // Rotation
        if (Input.GetKey(turnLeft)) transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
        if (Input.GetKey(turnRight)) transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
