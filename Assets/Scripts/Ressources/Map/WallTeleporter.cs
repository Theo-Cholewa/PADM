using UnityEngine;

public class WallTeleporter : MonoBehaviour
{
    public enum Axis { X, Z }
    public Axis directionAxis = Axis.X;

    [Tooltip("Valeur à ajouter ou soustraire quand un objet traverse ce mur")]
    public float teleportDistance = 300f;

    [Tooltip("Tags des objets à téléporter (par ex. 'Player', 'Ship')")]
    public string targetTag = "Blue";
    public string targetTag2 = "Red";

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("WallTeleporter triggered by: " + other.name);
        if (!other.CompareTag(targetTag) && !other.CompareTag(targetTag2))
            return;

        Vector3 pos = other.transform.position;

        // Applique le déplacement dans l’axe choisi
        if (directionAxis == Axis.X)
            pos.x += teleportDistance;
        else if (directionAxis == Axis.Z)
            pos.z += teleportDistance;

        other.transform.position = pos;
    }

    private void OnDrawGizmos()
    {
        // Pour bien visualiser dans la scène
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        Vector3 arrowDir = (directionAxis == Axis.X ? Vector3.right : Vector3.forward) * Mathf.Sign(teleportDistance);
        Gizmos.DrawRay(transform.position, arrowDir * 2);
    }
}