using System.Collections;
using System.Collections.Generic;
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

        // ✅ Correction : change l’alpha sans .WithAlpha()
        Color c = renderer.material.color;
        c.a = 1f - waterLevel;
        renderer.material.color = c;
    }
}
