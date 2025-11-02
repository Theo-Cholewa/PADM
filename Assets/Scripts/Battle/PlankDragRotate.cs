using UnityEngine;
using System.Linq;

public class PlankDragRotate : MonoBehaviour
{
    private Camera mainCamera;
    private bool isDragging = false;
    private float zCoord;

    private int[] activeTouchIds = new int[4] { -1, -1, -1, -1 };
    private const int REQUIRED_TOUCHES = 1;

    private Vector2[] lastPositions = new Vector2[4];

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

        Debug.Log("PlankDragRotate script démarré. Attente de 4 touches sur la planche.");
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

        if (touchCount > 0 && !isDragging)
        {
            //Debug.Log($"[UPDATE] Total de touches à l'écran : {touchCount}");
        }

        if (!isDragging && touchCount >= REQUIRED_TOUCHES)
        {
            TryStartDrag(touchCount);
        }

        if (isDragging)
        {
            ApplyMovementAndRotation(touchCount);
        }
    }

    private void TryStartDrag(int touchCount)
    {
        int foundTouches = 0;

        // Log Initial
        //Debug.Log($"[TRY START] Tentative de Démarrage. {REQUIRED_TOUCHES} touches requises.");

        for (int i = 0; i < touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = mainCamera.ScreenPointToRay(touch.position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit) && hit.transform == transform)
                {
                    // NOUVEAU LOG : Affiche une touche valide sur l'objet.
                    Debug.Log($"[TRY START] Touche trouvée sur la planche (ID: {touch.fingerId}).");

                    if (foundTouches < REQUIRED_TOUCHES)
                    {
                        activeTouchIds[foundTouches] = touch.fingerId;
                        lastPositions[foundTouches] = touch.position;
                        foundTouches++;

                        // NOUVEAU LOG : Affiche l'état du compteur.
                        Debug.Log($"[TRY START] Compteur : {foundTouches} / {REQUIRED_TOUCHES} touches valides sur la planche");
                    }

                    if (foundTouches == REQUIRED_TOUCHES)
                    {
                        isDragging = true;
                        zCoord = mainCamera.WorldToScreenPoint(transform.position).z;
                        SetHighlight(true); // <<<<<<<<<< ACTIVATION DE LA SURBRILLANCE
                        Debug.Log(">>>> DRAG PLANCHE DÉMARRÉ avec 4 touches.");
                        return;
                    }
                }
                else
                {
                    //Debug.Log($"[TRY START] Touche (ID: {touch.fingerId}) touchant un autre objet ou l'espace vide.");
                }
            }
        }

        if (!isDragging)
        {
            Debug.Log($"[TRY START] Fin de l'examen. Seulement {foundTouches} / {REQUIRED_TOUCHES} touches valides trouvées pour l'instant pour planche");
        }
    }

    private void ApplyMovementAndRotation(int touchCount)
    {
        Vector2[] currentPositions = new Vector2[REQUIRED_TOUCHES];
        int activeCount = 0;

        // Log pour le suivi du drag
        Debug.Log("[APPLY] Drag en cours...");

        for (int i = 0; i < touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            for (int j = 0; j < REQUIRED_TOUCHES; j++)
            {
                if (touch.fingerId == activeTouchIds[j])
                {
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        currentPositions[j] = touch.position;
                        activeCount++;
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        Debug.Log($"Plank Touch ID {touch.fingerId} levée/annulée.");
                        activeTouchIds[j] = -1;
                        currentPositions[j] = Vector2.zero;

                        // Le décompte est géré par la vérification finale activeCount < REQUIRED_TOUCHES
                    }
                }
            }
        }

        if (activeCount < REQUIRED_TOUCHES)
        {
            isDragging = false;
            SetHighlight(false); // <<<<<<<<<< DÉSACTIVATION DE LA SURBRILLANCE
            Debug.Log($"<<<< DRAG PLANCHE ARRÊTÉ : Moins de 4 touches sont actives ({activeCount} / {REQUIRED_TOUCHES}).");
            for (int i = 0; i < REQUIRED_TOUCHES; i++) activeTouchIds[i] = -1;
            return;
        }

        Vector2 centerOld = Vector2.zero;
        Vector2 centerNew = Vector2.zero;

        for (int i = 0; i < REQUIRED_TOUCHES; i++)
        {
            centerOld += lastPositions[i];
            centerNew += currentPositions[i];
        }

        centerOld /= REQUIRED_TOUCHES;
        centerNew /= REQUIRED_TOUCHES;

        Vector3 moveVector = GetWorldPosition(centerNew) - GetWorldPosition(centerOld);
        transform.position += moveVector;

        Vector2 leftCenterOld = (lastPositions[0] + lastPositions[1]) / 2f;
        Vector2 leftCenterNew = (currentPositions[0] + currentPositions[1]) / 2f;

        Vector2 rightCenterOld = (lastPositions[2] + lastPositions[3]) / 2f;
        Vector2 rightCenterNew = (currentPositions[2] + currentPositions[3]) / 2f;

        Vector2 plankVectorOld = rightCenterOld - leftCenterOld;
        Vector2 plankVectorNew = rightCenterNew - leftCenterNew;

        float angle = Vector2.SignedAngle(plankVectorOld, plankVectorNew);

        if (Mathf.Abs(angle) > 0.01f)
        {
            transform.Rotate(Vector3.up, angle, Space.World);
            Debug.Log($"Rotation de la planche appliquée : {angle:F2} degrés.");
        }

        for (int i = 0; i < REQUIRED_TOUCHES; i++)
        {
            lastPositions[i] = currentPositions[i];
        }
    }

    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        Vector3 point = new Vector3(screenPosition.x, screenPosition.y, zCoord);
        return mainCamera.ScreenToWorldPoint(point);
    }
}