using UnityEngine;

public class ChickenNet : MonoBehaviour
{
    [Header("Contrôles")]
    public float moveSpeed = 5f;

    [Header("Taille du filet (zone de capture)")]
    public float captureRadius = 1.5f;

    void Update()
    {
        // Déplacement simple du filet (ex : flèches ou ZQSD)
        float h = Input.GetAxis("Vertical");
        float v = -Input.GetAxis("Horizontal");

        Vector3 move = new Vector3(h, 0, v) * moveSpeed * Time.deltaTime;
        transform.position += move;

        // Vérifie la capture
        CheckCapture();
    }

    void CheckCapture()
    {
        // On cherche tous les poulets autour du filet
        Collider[] hits = Physics.OverlapSphere(transform.position, captureRadius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Chicken"))
            {
                Destroy(hit.gameObject); // capture !
                Debug.Log("🐔 Poulet capturé !");
            }
        }
    }

#if UNITY_EDITOR
    // Dessine la zone de capture dans la scène
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, captureRadius);
    }
#endif
}