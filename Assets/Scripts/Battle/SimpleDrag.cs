using UnityEngine;
using System.Linq;

public class SmoothDrag : MonoBehaviour
{
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 offset;
    private float zCoord;

    // Drag
    private int[] activeTouchIds = new int[4] { -1, -1, -1, -1 };
    private const int REQUIRED_TOUCHES = 1;

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

        // NOUVEAU LOG : Affiche le nombre total de touches à l'écran.
        if (touchCount > 0 && !isDragging)
        {
            Debug.Log($"[UPDATE] Total de touches à l'écran : {touchCount}. En attente du Drag (4 touches Began).");
        }

        // --- 1. GESTION DU DÉMARRAGE DU DRAG ---

        if (!isDragging && touchCount >= REQUIRED_TOUCHES)
        {
            // Log de tentative de démarrage
            //Debug.Log("[START DRAG TENTATIVE] L'écran a assez de touches. Vérification Raycast en cours...");

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

                        // Si on a déjà trouvé 4 touches, on ne prend plus en compte les suivantes dans ce même Update
                        if (foundTouches < REQUIRED_TOUCHES)
                        {
                            activeTouchIds[foundTouches] = touch.fingerId;
                            foundTouches++;

                            // NOUVEAU LOG : Affiche le compteur de touches valides sur l'objet
                            Debug.Log($"[START DRAG] Touche valide (ID {touch.fingerId}) trouvée sur l'objet bucket. Compteur : {foundTouches}/{REQUIRED_TOUCHES}");
                        }

                        if (foundTouches == REQUIRED_TOUCHES)
                        {
                            // 4 touches trouvées simultanément
                            Touch mainTouch = Input.GetTouch(i); // Utilise la dernière touche valide pour l'offset

                            zCoord = mainCamera.WorldToScreenPoint(transform.position).z;
                            offset = transform.position - GetWorldPosition(mainTouch.position);
                            isDragging = true;
                            SetHighlight(true); // <<<<<<<<<< ACTIVATION DE LA SURBRILLANCE
                            Debug.Log(">>>> DRAG DÉMARRÉ avec 4 touches sur le seau.");
                            break;
                        }
                    }
                    // Log si la touche est "Began" mais n'a pas touché cet objet
                    else
                    {
                        Debug.Log($"[START DRAG] Touche Began (ID {touch.fingerId}) n'a pas touché l'objet.");
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
                            Debug.Log($"Drag en cours par Touch ID {touch.fingerId}.");
                        }
                        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        {
                            Debug.Log($"Drag Touch ID {touch.fingerId} levée/annulée. Vérification de l'arrêt du Drag.");
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
                        // Vérifiez que la touche touche l'objet pour la rotation (sécurité)
                        Ray ray = mainCamera.ScreenPointToRay(touch.position);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit) && hit.transform == transform)
                        {
                            rotationTouchId = touch.fingerId;
                            lastRotationPosition = touch.position;
                            Debug.Log($"Rotation Touch ID {touch.fingerId} assignée. Début possible de la rotation.");
                        }
                    }

                    // C'est notre doigt de rotation
                    else if (touch.fingerId == rotationTouchId)
                    {
                        if (touch.phase == TouchPhase.Moved)
                        {
                            ApplyRotation(touch.position);
                            lastRotationPosition = touch.position;
                        }
                        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        {
                            Debug.Log($"Rotation Touch ID {rotationTouchId} levée/annulée. Rotation terminée.");
                            rotationTouchId = -1;
                        }
                    }
                }
            }

            // C. Arrêt du Drag
            if (!oneOfFourIsActive && activeTouchIds.All(id => id == -1))
            {
                isDragging = false;
                rotationTouchId = -1; // S'assurer que la rotation est aussi arrêtée
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
        transform.Rotate(Vector3.up, -angle);

        Debug.Log($"Rotation en cours (ID {rotationTouchId}). Angle appliqué : {angle:F2} degrés.");
    }
}