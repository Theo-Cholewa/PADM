using UnityEngine;

public class RotateText : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 50f; // Degrees per second

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}
