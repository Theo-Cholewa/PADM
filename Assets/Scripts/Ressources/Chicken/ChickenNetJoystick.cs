using UnityEngine;

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

    // 🔹 Bateau actuellement lié dynamiquement
    [HideInInspector] public ShipController linkedShip;

    void Update()
    {
        if (!CanMove())
            return;

        // Moyenne des directions des 4 joysticks
        Vector2 avgDir = GetAverageDirection();

        if (avgDir.sqrMagnitude > 0.01f)
        {
            Vector3 move = new Vector3(avgDir.x, 0, avgDir.y) * moveSpeed * Time.deltaTime;
            transform.position += move;
            CheckCapture();
        }
    }

    bool CanMove()
    {
        return topLeft.IsActive && topRight.IsActive && bottomLeft.IsActive && bottomRight.IsActive;
    }

    Vector2 GetAverageDirection()
    {
        Vector2 sum = Vector2.zero;
        int count = 0;

        if (topLeft.IsActive) { sum += topLeft.Direction; count++; }
        if (topRight.IsActive) { sum += topRight.Direction; count++; }
        if (bottomLeft.IsActive) { sum += bottomLeft.Direction; count++; }
        if (bottomRight.IsActive) { sum += bottomRight.Direction; count++; }

        if (count == 0) return Vector2.zero;
        return (sum / count).normalized;
    }

    void CheckCapture()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, captureRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Chicken"))
            {
                Destroy(hit.gameObject);

                if (linkedShip != null)
                {
                    ShipData shipData = linkedShip.GetComponent<ShipData>();
                    if (shipData != null)
                    {
                        shipData.AddResource("food", 1);
                        Debug.Log($"🐔 {linkedShip.team} a capturé un poulet — nourriture totale : {shipData.food}");
                    }
                }
                else
                {
                    Debug.Log("🐔 Poulet capturé (aucun bateau actuellement accosté).");
                }
            }
        }
    }

    // 🔹 Méthode appelée par le bateau ou l'île
    public void SetLinkedShip(ShipController ship)
    {
        linkedShip = ship;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, captureRadius);
    }
#endif
}