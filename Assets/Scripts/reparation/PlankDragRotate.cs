using UnityEngine;
using System.Linq;

public class PlankDragRotate : MonoBehaviour
{
    private Camera mainCamera;
    private bool isDragging = false;
    private float zCoord;

    // CHANGEMENT MAJEUR 1: 2 touches requises
    private int[] activeTouchIds = new int[2] { -1, -1 };
    private const int REQUIRED_TOUCHES = 2; // CHANGEMENT MAJEUR 2: 2 touches

    // NOUVEAU: Stocke les positions écran initiales des deux doigts
    private Vector2[] initialPositions = new Vector2[2];

    // NOUVEAU: Position du centre des doigts de la frame précédente pour le mouvement
    private Vector2 lastCenterPosition;

    // --- VARIABLES D'EFFET VISUEL ---
    public Material highlightMaterial;
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

        Debug.Log($"PlankDragRotate script démarré. Attente de {REQUIRED_TOUCHES} touche(s) pour le Drag.");
    }

    private void SetHighlight(bool highlight)
    {
        if (objectRenderer == null || highlightMaterial == null) return;

        if (highlight)
        {
            if (objectRenderer.material != highlightMaterial)
            {
                objectRenderer.material = highlightMaterial;
                Debug.Log("Surbrillance activée sur la Planche.");
            }
        }
        else
        {
            if (objectRenderer.material != defaultMaterial)
            {
                objectRenderer.material = defaultMaterial;
                Debug.Log("Surbrillance désactivée sur la Planche.");
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
            ApplyMovement(touchCount);
        }
    }

    private void TryStartDrag(int touchCount)
    {
        // On cherche seulement à enregistrer les touches valides
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
                            initialPositions[j] = touch.position; // ENREGISTRE LA POSITION
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
            isDragging = true;
            zCoord = mainCamera.WorldToScreenPoint(transform.position).z;

            // Calculer le centre initial des deux touches
            Vector2 center = (initialPositions[0] + initialPositions[1]) / REQUIRED_TOUCHES;
            lastCenterPosition = center;

            SetHighlight(true);
            Debug.Log($">>>> DRAG PLANCHE DÉMARRÉ avec {REQUIRED_TOUCHES} touche(s).");
            return;
        }
    }

    private void ApplyMovement(int touchCount)
    {
        Vector2 currentCenter = Vector2.zero;
        int activeDragTouchCount = 0;

        // 1. Collecter les positions des touches actives
        for (int i = 0; i < touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            for (int j = 0; j < REQUIRED_TOUCHES; j++)
            {
                if (touch.fingerId == activeTouchIds[j])
                {

                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        // Une touche de drag s'est terminée
                        Debug.Log($"Plank Touch ID {touch.fingerId} levée/annulée.");
                        activeTouchIds[j] = -1;
                    }
                    else
                    {
                        // Les touches Moved ou Stationary contribuent au centre
                        activeDragTouchCount++;
                        currentCenter += touch.position;
                    }
                    break;
                }
            }
        }

        // 2. Vérification d'arrêt et application du mouvement
        if (activeDragTouchCount < REQUIRED_TOUCHES)
        {
            // Arrêt du Drag
            for (int j = 0; j < activeTouchIds.Length; j++) activeTouchIds[j] = -1;
            isDragging = false;
            SetHighlight(false);
            Debug.Log("<<<< DRAG PLANCHE ARRÊTÉ : Le nombre de touches actives est insuffisant.");
            return;
        }

        // 3. Application du Mouvement (Translation)
        currentCenter /= REQUIRED_TOUCHES; // Calculer la moyenne du centre actuel

        Vector3 moveVector = GetWorldPosition(currentCenter) - GetWorldPosition(lastCenterPosition);
        transform.position += moveVector;

        // Mettre à jour la dernière position du centre pour la prochaine frame
        lastCenterPosition = currentCenter;
    }

    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        Vector3 point = new Vector3(screenPosition.x, screenPosition.y, zCoord);
        return mainCamera.ScreenToWorldPoint(point);
    }
}