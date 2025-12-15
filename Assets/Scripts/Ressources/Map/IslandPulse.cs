using UnityEngine;

public class IslandPulse : MonoBehaviour
{
    public Renderer islandRenderer;
    public Color baseColor = new Color(0.8f, 0.7f, 0.5f);
    public Color pulseColor = new Color(1f, 0.9f, 0.6f);
    public float speed = 1.5f;

    void Update()
    {
        float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f;
        Color current = Color.Lerp(baseColor, pulseColor, t);
        islandRenderer.material.color = current;
    }
}
