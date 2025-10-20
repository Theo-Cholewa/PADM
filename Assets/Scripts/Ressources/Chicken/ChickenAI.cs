using UnityEngine;

public class ChickenAI : MonoBehaviour
{
    [Header("Mouvement")]
    public float moveSpeed = 2f;          // Vitesse de déplacement
    public float rotationSpeed = 180f;    // Vitesse de rotation (en degrés/seconde)
    public float maxSegmentDistance = 5f; // distance max aléatoire pour chaque segment droit
    public float maxTurnAngle = 90f;      // angle maximum (en degrés) pour le pivot après un segment

    [Header("Zone de déplacement")]
    public float moveRadius = 10f;        // Rayon maximum autour du point d’origine
    private Vector3 originPos;

    private Vector3 moveDirection;
    private float remainingSegmentDistance = 0f;
    private bool forcingToCenter = false; // si vrai, le poulet est en train de revenir vers l'origine

    void Start()
    {
        originPos = transform.position;
        ChooseNewSegment();
    }

    void Update()
    {
        // Si on n'a plus de distance restante pour le segment, choisir le suivant
        if (remainingSegmentDistance <= 0f)
        {
            // si on venait de forcer le retour au centre, on réinitialise le flag et choisit un segment normal
            if (forcingToCenter)
            {
                forcingToCenter = false;
            }
            ChooseNewSegment();
        }

        // Fait tourner le poulet vers sa direction cible
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Avance dans la direction, sans dépasser la distance restante
        float step = moveSpeed * Time.deltaTime;
        float moveStep = Mathf.Min(step, remainingSegmentDistance);
        transform.position += transform.forward * moveStep;
        remainingSegmentDistance -= moveStep;

        // Si, après le déplacement, le poulet est en dehors du rayon autorisé, forcer le retour vers le centre
        if (Vector3.Distance(originPos, transform.position) > moveRadius)
        {
            ForceReturnToCenter();
        }
    }

    void ChooseNewSegment()
    {
        // Choisit une distance de segment aléatoire (>= 0.5 pour éviter micro-segments)
        float distance = Random.Range(0.5f, Mathf.Max(0.5f, maxSegmentDistance));

        // Choisit un angle de rotation aléatoire par rapport à l'orientation actuelle
        float angle = Random.Range(-maxTurnAngle, maxTurnAngle);
        Vector3 proposedDir = Quaternion.Euler(0f, angle, 0f) * transform.forward;

        // Vérifie si en avançant de 'distance' dans proposedDir le poulet sortirait du cercle
        float maxTravel = GetRayCircleIntersectionDistance(transform.position, proposedDir, originPos, moveRadius);

        if (maxTravel < 0f)
        {
            // pas d'intersection détectée (probablement entièrement à l'intérieur) -> on peut avancer
            moveDirection = proposedDir.normalized;
            remainingSegmentDistance = distance;
        }
        else
        {
            // si le trajet proposé dépasse la limite, on force un retour vers le centre
            if (distance > maxTravel)
            {
                ForceReturnToCenter();
            }
            else
            {
                moveDirection = proposedDir.normalized;
                remainingSegmentDistance = distance;
            }
        }
    }

    // Force le poulet à se diriger vers l'origine et définit la distance nécessaire
    void ForceReturnToCenter()
    {
        moveDirection = (originPos - transform.position).normalized;
        remainingSegmentDistance = Vector3.Distance(transform.position, originPos);
        forcingToCenter = true;
    }

    // Retourne la plus petite distance positive t telle que position + t*dir intersecte le cercle (origine, radius).
    // Si aucun t positif (pas d'intersection devant), retourne -1.
    float GetRayCircleIntersectionDistance(Vector3 position, Vector3 dir, Vector3 circleCenter, float radius)
    {
        Vector3 p = position - circleCenter;
        Vector3 d = dir.normalized;
        float a = Vector3.Dot(d, d); // =1
        float b = 2f * Vector3.Dot(d, p);
        float c = Vector3.Dot(p, p) - radius * radius;
        float disc = b * b - 4f * a * c;
        if (disc < 0f) return -1f; // pas d'intersection
        float sqrtD = Mathf.Sqrt(disc);
        float t1 = (-b - sqrtD) / (2f * a);
        float t2 = (-b + sqrtD) / (2f * a);
        // on veut la plus petite t positive
        float t = float.MaxValue;
        if (t1 > 0f) t = Mathf.Min(t, t1);
        if (t2 > 0f) t = Mathf.Min(t, t2);
        if (t == float.MaxValue) return -1f;
        return t;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.3f);
        Gizmos.DrawWireSphere(Application.isPlaying ? originPos : transform.position, moveRadius);
    }
#endif
}