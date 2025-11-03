using UnityEngine;
using System.Linq;

public class SmoothDrag : MonoBehaviour
{
    private Camera mainCamera;
    private bool isDragging = false;
    private float zCoord;

    // DRAG (TRANSLATION) : 2 touches requises
    private int[] activeTouchIds = new int[2] { -1, -1 };
    private const int REQUIRED_TOUCHES = 2;

    // NOUVEAU: Stocke les positions écran initiales des deux doigts pour le calcul du centre
    private Vector2[] initialPositions = new Vector2[2];

    // Utilisé pour stocker la position du centre des doigts de la frame précédente (pour le mouvement)
    private Vector2 lastCenterPosition;

    // ROTATION : 1 doigt supplémentaire (comme avant)
    private int rotationTouchId = -1;
    private Vector2 lastRotationPosition; // Position du doigt de rotation

    // NOUVEAU: État pour la rotation de retour
    private bool isRotatingBack = false;
    private Quaternion initialRotation; // Rotation de départ du seau
    public float returnSpeed = 180f; // Vitesse de retour en degrés/seconde

    public Material highlightMaterial;
    private Renderer objectRenderer;
    private Material defaultMaterial;

    private Quaternion dragStartRotation;
    private BucketPlane bucketPlaneScript;

    void Start()
    {
        mainCamera = Camera.main;

        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            defaultMaterial = objectRenderer.material;
        }

        bucketPlaneScript = GetComponent<BucketPlane>();
        if (bucketPlaneScript == null)
        {
            Debug.LogError("BucketPlane script non trouvé sur le même GameObject !");
        }

        // ENREGISTRE LA ROTATION INITIALE
        initialRotation = transform.rotation;

        Debug.Log($"SmoothDrag script démarré. Attente de {REQUIRED_TOUCHES} touche(s) pour le Drag.");
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

        if (!isDragging && touchCount >= REQUIRED_TOUCHES)
        {
            TryStartDrag(touchCount);
        }

        if (isDragging)
        {
            ApplyMovementAndRotation(touchCount);
        }

        // NOUVEAU: Gérer le retour de rotation si nécessaire
        if (isRotatingBack)
        {
            RotateBack();
        }
    }

    private void TryStartDrag(int touchCount)
    {
        // On cherche seulement à enregistrer les 2 premières touches valides
        int foundTouches = activeTouchIds.Count(id => id != -1);

        for (int i = 0; i < touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = mainCamera.ScreenPointToRay(touch.position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit) && hit.transform == transform)
                {
                    // Chercher un slot libre et enregistrer la touche et sa position
                    for (int j = 0; j < REQUIRED_TOUCHES; j++)
                    {
                        if (activeTouchIds[j] == -1)
                        {
                            activeTouchIds[j] = touch.fingerId;
                            initialPositions[j] = touch.position;
                            foundTouches++;
                            break;
                        }
                    }
                }
            }
        }

        // Si toutes les touches requises sont là
        if (foundTouches == REQUIRED_TOUCHES)
        {
            // Arrêter immédiatement le retour de rotation si l'utilisateur recommence à draguer
            isRotatingBack = false;

            zCoord = mainCamera.WorldToScreenPoint(transform.position).z;
            isDragging = true;
            dragStartRotation = transform.rotation;

            // Calculer le centre initial des deux touches
            Vector2 center = (initialPositions[0] + initialPositions[1]) / REQUIRED_TOUCHES;
            lastCenterPosition = center;

            SetHighlight(true);
            Debug.Log($">>>> DRAG DÉMARRÉ avec {REQUIRED_TOUCHES} touche(s) sur le seau.");
        }
    }

    private void ApplyMovementAndRotation(int touchCount)
    {
        Vector2 currentCenter = Vector2.zero;
        Vector2[] currentPositions = new Vector2[REQUIRED_TOUCHES];
        int activeDragTouchCount = 0;

        bool isAnyDragTouchActive = false;

        // --- GESTION DU MOUVEMENT (2 DOIGTS) ET DES ÉVÉNEMENTS (3e DOIGT) ---
        for (int i = 0; i < touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            bool isDragTouch = false;

            // 1. VÉRIFICATION DES TOUCHES DE DRAG (MOUVEMENT)
            for (int j = 0; j < REQUIRED_TOUCHES; j++)
            {
                if (touch.fingerId == activeTouchIds[j])
                {
                    isDragTouch = true;
                    isAnyDragTouchActive = true;

                    currentPositions[j] = touch.position;

                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        activeTouchIds[j] = -1;
                        Debug.Log($"Bucket Drag Touch ID {touch.fingerId} levée/annulée.");
                    }
                    else
                    {
                        activeDragTouchCount++;
                        currentCenter += touch.position;
                    }
                    break;
                }
            }

            // 2. VÉRIFICATION DE LA TOUCHE DE ROTATION (3e DOIGT)
            if (isAnyDragTouchActive && !isDragTouch)
            {
                // Tente d'assigner un nouveau doigt à la rotation (Began)
                if (rotationTouchId == -1 && touch.phase == TouchPhase.Began)
                {
                    // Arrêter le retour de rotation si le doigt de rotation est posé
                    isRotatingBack = false;

                    Ray ray = mainCamera.ScreenPointToRay(touch.position);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit) && hit.transform == transform)
                    {
                        rotationTouchId = touch.fingerId;
                        lastRotationPosition = touch.position;
                        Debug.Log($"Rotation Touch ID {touch.fingerId} assignée. Début possible de la rotation.");
                    }
                }
                // Si c'est notre doigt de rotation assigné (Moved/Ended)
                else if (touch.fingerId == rotationTouchId)
                {
                    if (touch.phase == TouchPhase.Moved)
                    {
                        ApplyRotation(touch.position);
                        lastRotationPosition = touch.position;
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        // NOUVEAU: Déclencher le retour de rotation
                        isRotatingBack = true;

                        Debug.Log($"Rotation Touch ID {rotationTouchId} levée/annulée. Retour à la rotation initiale déclenché.");
                        rotationTouchId = -1;
                    }
                }
            }
        }

        // C. Arrêt du Drag (Si le nombre de touches de drag est insuffisant)
        if (activeDragTouchCount < REQUIRED_TOUCHES)
        {
            // Nettoyage complet
            for (int j = 0; j < activeTouchIds.Length; j++) activeTouchIds[j] = -1;
            rotationTouchId = -1;
            isDragging = false;
            SetHighlight(false);
            Debug.Log("<<<< DRAG ARRÊTÉ : Le nombre de touches actives est insuffisant.");
            return;
        }

        // --- APPLICATION DU MOUVEMENT (Si le drag est toujours actif) ---
        currentCenter /= REQUIRED_TOUCHES;

        // Mouvement (Translation)
        Vector3 moveVector = GetWorldPosition(currentCenter) - GetWorldPosition(lastCenterPosition);
        transform.position += moveVector;

        // Mettre à jour la dernière position du centre pour la prochaine frame
        lastCenterPosition = currentCenter;
    }

    // NOUVEAU: Logique de retour progressif à la rotation initiale
    private void RotateBack()
    {
        // Utilise RotateTowards pour un retour en douceur
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            initialRotation,
            returnSpeed * Time.deltaTime
        );

        // Si la rotation est presque terminée, fixer la rotation et arrêter le processus
        if (Quaternion.Angle(transform.rotation, initialRotation) < 0.1f)
        {
            transform.rotation = initialRotation;
            isRotatingBack = false;
            Debug.Log("Rotation revenue à l'état initial.");
        }
        // IMPORTANT: La logique de vidage/remplissage dans BucketPlane.cs pourrait interférer ici
        // si elle vérifie `transform.rotation` directement.
    }


    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        Vector3 point = new Vector3(screenPosition.x, screenPosition.y, zCoord);
        return mainCamera.ScreenToWorldPoint(point);
    }

    // Ancienne logique de rotation à 1 doigt (comparaison du mouvement de la touche par rapport au centre du seau)
    private void ApplyRotation(Vector2 currentPosition)
    {
        // Arrêter le retour de rotation si on recommence à tourner
        isRotatingBack = false;

        Vector2 bucketScreenPosition = mainCamera.WorldToScreenPoint(transform.position);

        Vector2 vectorOld = lastRotationPosition - bucketScreenPosition;
        Vector2 vectorNew = currentPosition - bucketScreenPosition;

        float angle = Vector2.SignedAngle(vectorOld, vectorNew);

        // Appliquer la rotation autour de l'axe Y
        transform.Rotate(Vector3.up, -angle);

        // --- Logique de Vidage (basée sur l'angle total d'inclinaison) ---
        if (bucketPlaneScript != null && !bucketPlaneScript.rotated)
        {
            float angleDifference = Quaternion.Angle(dragStartRotation, transform.rotation);

            // Si le seau est tourné de 90 degrés par rapport à l'état initial
            if (angleDifference >= 90f)
            {
                bucketPlaneScript.InitiateRotation();
            }
        }
    }
}
