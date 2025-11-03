using Unity.VisualScripting;
using UnityEngine;
public class Ship : MonoBehaviour
{
    public float speed = 0f;

    private float waterLevel = 0f;

    public Renderer renderer;

    void FixedUpdate()
    {
        if (speed > 0f) waterLevel += speed / 10000f;
        else waterLevel -= 4f / 10000f;
        if (waterLevel < 0f) waterLevel = 0f;
        var color = renderer.material.color;
        color.a = 1f - waterLevel;
        renderer.material.color = color;
    }
}

