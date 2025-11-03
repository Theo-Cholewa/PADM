using UnityEngine;

public class BucketPlane : MonoBehaviour
{
    public GameObject water;
    public Transform hole;
    public GameObject puddle;
    public float showDistance = 1f;
    public float rotationSpeed = 90f;

    private bool waterVisible = false;
    private bool rotating = false;
    private Quaternion targetRotation;
    public bool rotated { get; private set; } // CHANGEMENT : Rendu public pour l'accès externe

    private Quaternion initialRotation;
    private Renderer puddleRenderer;

    void Start()
    {
        if (water != null)
            water.SetActive(false);

        initialRotation = transform.rotation;

        if (puddle != null)
            puddleRenderer = puddle.GetComponent<Renderer>();
    }

    void Update()
    {
        if (hole == null || water == null) return;

        bool puddleVisible = puddleRenderer != null && puddleRenderer.isVisible;

        // --- Logique d'activation de l'eau (collecte) lorsque le seau est proche du puddle/hole ---
        if (!waterVisible && puddleVisible)
        {
            float distance = Vector3.Distance(transform.position, hole.position);
            if (distance < showDistance)
            {
                water.SetActive(true);
                waterVisible = true;

                Debug.Log("💧 Water collected");

                if (puddle != null)
                {
                    puddle.SetActive(false);
                }

                if (hole != null && !hole.gameObject.activeSelf)
                {
                    hole.gameObject.SetActive(true);
                }
            }
        }

        // --- Contrôle manuel de la rotation (via 'Q' pour les tests) ---
        if (Input.GetKeyDown(KeyCode.Q) && !rotating)
        {
            if (!rotated)
            {
                InitiateRotation();
            }
            else
            {
                // Retour à la position initiale
                targetRotation = initialRotation;
                rotating = true;
                rotated = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (water != null && !waterVisible)
            {
                water.SetActive(true);
                waterVisible = true;
                Debug.Log("💧 Water manually activated (W)");
            }
        }

        // --- Rotation progressive ---
        if (rotating)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.01f)
            {
                transform.rotation = targetRotation;
                rotating = false;
            }
        }

        // --- Vidage du seau après rotation ---
        if (rotated && waterVisible && water != null)
        {
            water.SetActive(false);
            waterVisible = false;
            Debug.Log("💦 Water emptied (manual or touch rotation)");
        }

        // La logique de collecte est répétée ici, je l'ai commentée car elle est déjà au début de l'Update

        if (!waterVisible && puddleVisible)
        {
            float distance = Vector3.Distance(transform.position, hole.position);
            if (distance < showDistance)
            {
                water.SetActive(true);
                waterVisible = true;
                Debug.Log("💧 Water collected");
                Debug.Log($"[DÉBOGAGE HIERARCHIE] Parent (BucketPlane) est actif: {gameObject.activeInHierarchy}. L'objet Water est actif: {water.activeSelf}");

                // faire disparaître la flaque
                if (puddle != null) puddle.SetActive(false);

                // faire réapparaître le hole
                if (hole != null) hole.gameObject.SetActive(true);
            }
        }

    }

    // NOUVEAU : Méthode appelée par SmoothDrag pour initier la rotation de vidage
    public void InitiateRotation()
    {
        if (!rotating && !rotated)
        {
            // La rotation que vous avez définie
            targetRotation = Quaternion.Euler(179.286f, -89.99799f, -270.064f);
            rotating = true;
            rotated = true;
            Debug.Log("Rotation de vidage initiée par le drag tactile ou la touche 'Q'.");
        }
    }
}