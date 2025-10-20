using UnityEngine;
using System.Collections.Generic;

public class ChickenNetTouch : MonoBehaviour
{
    public enum ControlMode { Normal, Demo }
    [Header("Mode de contrôle")]
    public ControlMode mode = ControlMode.Normal;

    [Header("Références boutons (coins du filet)")]
    public TouchButton topLeft;
    public TouchButton topRight;
    public TouchButton bottomLeft;
    public TouchButton bottomRight;

    [Header("Déplacement du filet")]
    public float moveSpeed = 1f;

    [Header("Zone de capture")]
    public float captureRadius = 1.5f;

    // internal
    Vector3 lastAverageWorldPos;
    bool hadLastPos = false;

    void Update()
    {
        bool canMove = CanMove();
        if (canMove)
        {
            Vector2 avgScreen = GetAverageScreenPosition();
            if (avgScreen == Vector2.zero) return;

            Vector3 worldPos = ScreenToWorldOnNetPlane(avgScreen);

            if (hadLastPos)
            {
                Vector3 deltaWorld = worldPos - lastAverageWorldPos;

                // si delta très petit, ignore
                if (deltaWorld.sqrMagnitude > 1e-6f)
                {
                    // on ne veut que déplacer sur XZ
                    Vector3 move = new Vector3(deltaWorld.x, 0f, deltaWorld.z) * moveSpeed;
                    transform.position += move;

                    // check capture après déplacement
                    CheckCapture();
                }
            }

            lastAverageWorldPos = worldPos;
            hadLastPos = true;
        }
        else
        {
            // réinitialiser si on ne peut pas bouger
            hadLastPos = false;
        }
    }

    bool CanMove()
    {
        bool tl = topLeft != null && topLeft.IsPressed;
        bool tr = topRight != null && topRight.IsPressed;
        bool bl = bottomLeft != null && bottomLeft.IsPressed;
        bool br = bottomRight != null && bottomRight.IsPressed;

        if (mode == ControlMode.Normal) return tl && tr && bl && br;
        return tl || tr || bl || br;
    }

    Vector2 GetAverageScreenPosition()
    {
        List<Vector2> pos = new List<Vector2>();
        if (topLeft != null && topLeft.IsPressed) pos.Add(topLeft.TouchPosition);
        if (topRight != null && topRight.IsPressed) pos.Add(topRight.TouchPosition);
        if (bottomLeft != null && bottomLeft.IsPressed) pos.Add(bottomLeft.TouchPosition);
        if (bottomRight != null && bottomRight.IsPressed) pos.Add(bottomRight.TouchPosition);

        if (pos.Count == 0) return Vector2.zero;

        Vector2 avg = Vector2.zero;
        foreach (var p in pos) avg += p;
        avg /= pos.Count;
        return avg;
    }

    Vector3 ScreenToWorldOnNetPlane(Vector2 screenPoint)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Aucune Camera.main trouvée.");
            return transform.position;
        }

        // Plan parallèle XZ passant par la hauteur du filet
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

        Ray ray = cam.ScreenPointToRay(screenPoint);
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hit = ray.GetPoint(enter);
            return hit;
        }

        // --- fallback : conversion approximative via camera axes projetées sur XZ ---
        // utile si le rayon ne coupe pas le plan (peu probable)
        Vector3 camRight = cam.transform.right;
        Vector3 camUp = cam.transform.up; // direction écran y
        // projeter sur XZ
        camRight.y = 0f;
        camUp.y = 0f;
        if (camRight.sqrMagnitude < 1e-6f || camUp.sqrMagnitude < 1e-6f)
        {
            // dernier recours : tiny move
            return transform.position;
        }
        camRight.Normalize();
        camUp.Normalize();

        // estimer une distance en unités monde depuis un delta écran
        // facteur empirique (ajuste si besoin)
        float factor = 0.02f * (cam.transform.position - transform.position).magnitude;

        // on transforme le screenPoint depuis le centre d'écran
        Vector2 screenCenter = new Vector2(Screen.width, Screen.height) * 0.5f;
        Vector2 screenDelta = (screenPoint - screenCenter);

        Vector3 approx = transform.position + camRight * screenDelta.x * factor + camUp * screenDelta.y * factor;
        approx.y = transform.position.y;
        return approx;
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