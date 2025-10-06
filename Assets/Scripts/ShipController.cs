using UnityEngine;

public class ShipController : MonoBehaviour
{
    [Header("Infos Joueur")]
    public string playerName = "Red"; // juste pour organiser

    [Header("Touches de contrôle (configurables en Inspector)")]
    public KeyCode moveForward = KeyCode.Z;
    public KeyCode moveBackward = KeyCode.S;
    public KeyCode turnLeft = KeyCode.Q;
    public KeyCode turnRight = KeyCode.D;

    [Header("Paramètres de mouvement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;

    void Update()
    {
        Vector3 moveDir = Vector3.zero;

        if (Input.GetKey(moveForward)) moveDir += Vector3.forward;
        if (Input.GetKey(moveBackward)) moveDir -= Vector3.forward;

        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.Self);

        if (Input.GetKey(turnLeft)) transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
        if (Input.GetKey(turnRight)) transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}