using UnityEngine;
using System.Collections.Generic;

public class ChickenNetJoystick : MonoBehaviour
{
    [Header("Références des joysticks (coins du filet)")]
    public FixedJoystick topLeft;
    public FixedJoystick topRight;
    public FixedJoystick bottomLeft;
    public FixedJoystick bottomRight;

    [Header("Déplacement du filet")]
    public float moveSpeed = 2f;

    [Header("Zone de capture")]
    public float captureRadius = 1.5f;

    // vitesse minimale quand les joysticks divergent complètement
    [Range(0f, 1f)]
    public float minSpeedFactor = 0.5f;

    void Update()
    {
        if (!CanMove())
            return;

        // Moyenne des directions
        Vector2 avgDir = GetAverageDirection(out List<Vector2> directions);

        if (avgDir.sqrMagnitude > 0.01f)
        {
            // Calcul de la cohérence des angles
            float speedFactor = ComputeDirectionSimilarity(directions, avgDir);

            // Conversion vers un déplacement sur le plan XZ
            Vector3 move = new Vector3(avgDir.x, 0, avgDir.y) * moveSpeed * speedFactor * Time.deltaTime;

            transform.position += move;
            CheckCapture();
        }
    }

    bool CanMove()
    {
        return topLeft.IsActive && topRight.IsActive && bottomLeft.IsActive && bottomRight.IsActive;
    }

    Vector2 GetAverageDirection(out List<Vector2> dirs)
    {
        dirs = new List<Vector2>();

        if (topLeft.IsActive) dirs.Add(topLeft.Direction.normalized);
        if (topRight.IsActive) dirs.Add(topRight.Direction.normalized);
        if (bottomLeft.IsActive) dirs.Add(bottomLeft.Direction.normalized);
        if (bottomRight.IsActive) dirs.Add(bottomRight.Direction.normalized);

        if (dirs.Count == 0) return Vector2.zero;

        Vector2 avg = Vector2.zero;
        foreach (var d in dirs) avg += d;
        avg /= dirs.Count;

        return avg.normalized;
    }

    float ComputeDirectionSimilarity(List<Vector2> directions, Vector2 avgDir)
    {
        if (directions.Count == 0) return 1f;

        float avgAngle = Mathf.Atan2(avgDir.y, avgDir.x);
        float totalDiff = 0f;

        foreach (var dir in directions)
        {
            float angle = Mathf.Atan2(dir.y, dir.x);
            float diff = Mathf.Abs(Mathf.DeltaAngle(Mathf.Rad2Deg * avgAngle, Mathf.Rad2Deg * angle));
            totalDiff += diff;
        }

        float meanDiff = totalDiff / directions.Count;

        // 0° → vitesse = 100 %
        // 90° → vitesse = minSpeedFactor (50 % par défaut)
        float t = Mathf.Clamp01(meanDiff / 90f); // interpolation de 0 à 1
        float speedFactor = Mathf.Lerp(1f, minSpeedFactor, t);

        return speedFactor;
    }

    void CheckCapture()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, captureRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Chicken"))
            {
                Destroy(hit.gameObject);
                Debug.Log("🐔 Poulet capturé !");
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, captureRadius);
    }
#endif
}
