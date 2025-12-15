using UnityEngine;

public class ChickenNet : MonoBehaviour
{

    [Header("Taille du filet (zone de capture)")]
    public float captureRadius = 1.5f;

    void Update()
    {

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
                RessourceClient.current.Get(Team.BLUE).Add(RessourceType.Chicken,1); // TODO: Make it use the team.
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