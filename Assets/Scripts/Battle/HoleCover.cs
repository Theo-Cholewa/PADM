using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Ajouté pour les requêtes LINQ si nécessaire, mais non utilisé ici.

public class HoleCover : MonoBehaviour
{
    public GameObject holeObject;
    public GameObject puddleObject;

    // Pour le clouage
    public int requiredTaps = 2;
    // La variable tapRadius n'est plus utilisée pour la vérification de la zone, mais reste pour la compatibilité (retirée du code actif).
    public float tapRadius = 1.5f;
    public float tapTimeout = 1.0f;

    // Variables ajoutées pour l'accès à la caméra et à la profondeur (z)
    private Camera mainCamera;
    private float zCoord;

    private bool isCovering = false;
    private bool isFixed = false;

    // Logique de Clouage
    private int leftTaps = 0; // Ces variables ne sont plus nécessaires individuellement
    private float leftLastTapTime = 0f;
    private int rightTaps = 0; // Ces variables ne sont plus nécessaires individuellement
    private float rightLastTapTime = 0f;

    // NOUVEAU: Compteur unique pour le clouage
    private int totalTaps = 0;
    private float lastTapTime = 0f;

    // Positions locales des points de clouage (ne servent plus au tapotement)
    public Vector3 leftEndLocalPos = new Vector3(-0.5f, 0, 0);
    public Vector3 rightEndLocalPos = new Vector3(0.5f, 0, 0);

    void Start()
    {
        mainCamera = Camera.main;
        // On définit le plan Z pour la conversion ScreenToWorld
        zCoord = mainCamera.WorldToScreenPoint(transform.position).z;

        // Configure le Rigidbody en mode Cinématique
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            Debug.Log("Rigidbody de la planche configuré en mode Cinématique pour un drag contrôlé.");
        }
    }

    void Update()
    {
        if (isFixed) return;

        if (isCovering)
        {
            CheckTaps();
        }

        // Vérification du clouage : utiliser le totalTaps
        if (totalTaps >= requiredTaps)
        {
            FixPlank();
        }

        // Réinitialisation du compteur unique de tapotements
        if (Time.time > lastTapTime + tapTimeout) totalTaps = 0;
    }

    // NOUVEAU: Pour visualiser le rayon de détection dans l'éditeur (Gizmos)
    private void OnDrawGizmosSelected()
    {
        if (transform != null)
        {
            // Dessine les points de clouage et leur rayon de détection
            Gizmos.color = Color.yellow;
            Vector3 leftPos = transform.TransformPoint(leftEndLocalPos);
            Vector3 rightPos = transform.TransformPoint(rightEndLocalPos);

            // Le tapRadius n'est plus utilisé pour le tapotement, mais on le laisse pour la visualisation des points initiaux
            Gizmos.DrawWireSphere(leftPos, 0.2f);
            Gizmos.DrawWireSphere(rightPos, 0.2f);

            // Ligne entre les deux points pour la référence
            Gizmos.color = Color.red;
            Gizmos.DrawLine(leftPos, rightPos);
        }
    }

    private void CheckTaps()
    {
        // On n'a plus besoin des positions mondiales des extrémités
        // Vector3 leftEndWorldPos = transform.TransformPoint(leftEndLocalPos);
        // Vector3 rightEndWorldPos = transform.TransformPoint(rightEndLocalPos);

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            // Nous vérifions uniquement la fin de la touche (un tapotement est un Ended très court)
            if (touch.phase == TouchPhase.Ended)
            {
                // NOUVEAU: Utilisation d'un Raycast pour vérifier si le tapotement a touché la planche
                Ray ray = mainCamera.ScreenPointToRay(touch.position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    // Si le Raycast touche CET objet (la planche)
                    if (hit.transform == transform)
                    {
                        if (Time.time < lastTapTime + tapTimeout)
                        {
                            totalTaps++;
                            Debug.Log($"✅ Tapotement sur Planche (CLOUAGE) détecté. Total: {totalTaps}/{requiredTaps}");
                        }
                        else
                        {
                            totalTaps = 1; // Premier tapotement
                        }
                        lastTapTime = Time.time;
                        return; // On a compté un tap, on sort de la boucle de touches pour ne pas compter deux fois dans la même frame
                    }
                }

                // LOG DE DÉBOGAGE si la touche est "Ended" mais n'a pas touché la planche
                Debug.Log($"[DEBUG TAP] Tapotement hors zone. Le Raycast n'a pas touché la planche.");
            }
        }
    }

    private void FixPlank()
    {
        isFixed = true;

        if (holeObject != null)
        {
            holeObject.SetActive(false);
            Debug.Log(">>>> TROU REBOUCHÉ ET CLOUÉ : Le trou a été désactivé !");
        }

        if (puddleObject != null && puddleObject.activeSelf)
        {
            puddleObject.SetActive(false);
        }

        if (gameObject.GetComponent<PlankDragRotate>() != null)
        {
            gameObject.GetComponent<PlankDragRotate>().enabled = false;
        }

        // NOUVEAU : Désactiver le GameObject de la planche elle-même.
        gameObject.SetActive(false);
        Debug.Log("La planche a été désactivée après clouage.");
    }

    private void OnTriggerStay(Collider other)
    {
        // LOG DE DÉBOGAGE
        Debug.Log($"[DEBUG COLLISION] OnTriggerStay a été appelé avec l'objet : {other.gameObject.name}.");

        // IMPORTANT: Vérifie que la collision est avec l'objet trou
        if (holeObject != null && other.gameObject == holeObject)
        {
            if (!isCovering)
            {
                isCovering = true;
                Debug.Log("🎯 OK ma planche est au dessus du trou !"); // LOG DE SUCCÈS
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (holeObject != null && other.gameObject == holeObject)
        {
            isCovering = false;
            Debug.Log("Planche éloignée du trou. Compteur de clouage réinitialisé.");
            totalTaps = 0; // Réinitialisation du compteur unique
            lastTapTime = 0f;
        }
    }
}
