using UnityEngine;

public class SmoothDrag : MonoBehaviour
{
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 offset;
    private float zCoord;

    // Drag
    private int[] activeTouchIds = new int[4] { -1, -1, -1, -1 };
    private const int REQUIRED_TOUCHES = 4;

    // Rotation
    private int rotationTouchId = -1;
    private Vector2 lastRotationPosition;

    // --- VARIABLES D'EFFET VISUEL ---
    public Material highlightMaterial; // À assigner dans l'Inspecteur
    private Renderer objectRenderer;
    private Material defaultMaterial;

    void Start()
    {
        mainCamera = Camera.main;

        // Initialisation des propriétés du Renderer
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            defaultMaterial = objectRenderer.material;
        }

        Debug.Log("SmoothDrag script démarré. Attente de 4 touches pour le Drag.");
    }

    private void SetHighlight(bool highlight)
    {
        if (objectRenderer == null || highlightMaterial == null) return;

        if (highlight)
        {
            if (objectRenderer.material != highlightMaterial)
            {
                objectRenderer.material = highlightMaterial;
                Debug.Log("Surbrillance activée sur le Bucket.");
            }
        }
        else
        {
            if (objectRenderer.material != defaultMaterial)
            {
                objectRenderer.material = defaultMaterial;
                Debug.Log("Surbrillance désactivée sur le Bucket.");
            }
        }
    }

    void Update()
    {
        int touchCount = Input.touchCount;

        // --- 1. GESTION DU DÉMARRAGE DU DRAG ---

        if (!isDragging && touchCount >= REQUIRED_TOUCHES)
        {
            int foundTouches = 0;

            for (int i = 0; i < touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (touch.phase == TouchPhase.Began)
                {
                    Ray ray = mainCamera.ScreenPointToRay(touch.position);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit) && hit.transform == transform)
                    {
                        // Touche sur l'objet trouvée
                        activeTouchIds[foundTouches] = touch.fingerId;
                        foundTouches++;
                        Debug.Log($"Touch ID {touch.fingerId} (slot {foundTouches}) trouvée sur l'objet.");

                        if (foundTouches == REQUIRED_TOUCHES)
                        {
                            // 4 touches trouvées simultanément
                            Touch mainTouch = Input.GetTouch(i);

                            zCoord = mainCamera.WorldToScreenPoint(transform.position).z;
                            offset = transform.position - GetWorldPosition(mainTouch.position);
                            isDragging = true;
                            SetHighlight(true); // <<<<<<<<<< ACTIVATION DE LA SURBRILLANCE
                            Debug.Log(">>>> DRAG DÉMARRÉ avec 4 touches sur le seau.");
                            break;
                        }
                    }
                }
            }
        }

        // --- 2. GESTION DU DRAG ET DE LA ROTATION ---

        if (isDragging)
        {
            bool oneOfFourIsActive = false;

            for (int i = 0; i < touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                bool isDragTouch = false;

                // A. Vérifier les 4 doigts de Drag
                for (int j = 0; j < REQUIRED_TOUCHES; j++)
                {
                    if (touch.fingerId == activeTouchIds[j])
                    {
                        isDragTouch = true;
                        oneOfFourIsActive = true;

                        if (touch.phase == TouchPhase.Moved)
                        {
                            transform.position = GetWorldPosition(touch.position) + offset;
                            // Optionnel : Debug.Log($"Drag en cours par Touch ID {touch.fingerId}.");
                        }
                        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        {
                            Debug.Log($"Drag Touch ID {touch.fingerId} levée/annulée.");
                            activeTouchIds[j] = -1;
                        }
                        break;
                    }
                }

                // B. Vérifier le doigt de Rotation
                if (!isDragTouch)
                {
                    // Tente d'assigner un nouveau doigt à la rotation
                    if (rotationTouchId == -1 && touch.phase == TouchPhase.Began)
                    {
                        rotationTouchId = touch.fingerId;
                        lastRotationPosition = touch.position;
                        Debug.Log($"Rotation Touch ID {touch.fingerId} assignée. Début possible de la rotation.");
                    }

                    // C'est notre doigt de rotation
                    else if (touch.fingerId == rotationTouchId)
                    {
                        if (touch.phase == TouchPhase.Moved)
                        {
                            ApplyRotation(touch.position);
                            lastRotationPosition = touch.position;
                            // La fonction ApplyRotation contient déjà un log détaillé
                        }
                        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        {
                            Debug.Log($"Rotation Touch ID {rotationTouchId} levée/annulée. Rotation terminée.");
                            rotationTouchId = -1;
                        }
                    }
                    // Optionnel : Un autre doigt que les 4 de drag ET le 5e de rotation est là.
                    // else if (touch.phase == TouchPhase.Began) { Debug.Log($"Touch ID {touch.fingerId} est une touche externe (ignorée pour drag/rotation)."); }
                }
            }

            // C. Arrêt du Drag
            if (!oneOfFourIsActive &&
                activeTouchIds[0] == -1 && activeTouchIds[1] == -1 && activeTouchIds[2] == -1 && activeTouchIds[3] == -1)
            {
                isDragging = false;
                rotationTouchId = -1;
                SetHighlight(false); // <<<<<<<<<< DÉSACTIVATION DE LA SURBRILLANCE
                Debug.Log("<<<< DRAG ARRÊTÉ : Les 4 touches de drag ont été levées.");
            }
        }
    }

    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        Vector3 point = new Vector3(screenPosition.x, screenPosition.y, zCoord);
        return mainCamera.ScreenToWorldPoint(point);
    }

    private void ApplyRotation(Vector2 currentPosition)
    {
        Vector2 bucketScreenPosition = mainCamera.WorldToScreenPoint(transform.position);

        Vector2 vectorOld = lastRotationPosition - bucketScreenPosition;
        Vector2 vectorNew = currentPosition - bucketScreenPosition;

        float angle = Vector2.SignedAngle(vectorOld, vectorNew);

        // Appliquer la rotation
        transform.Rotate(Vector3.up, angle);

        Debug.Log($"Rotation en cours (ID {rotationTouchId}). Angle appliqué : {angle:F2} degrés.");
    }
}