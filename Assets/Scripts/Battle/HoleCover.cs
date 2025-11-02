using UnityEngine;
using System.Collections.Generic;

public class HoleCover : MonoBehaviour
{
    public GameObject holeObject;
    public GameObject puddleObject;

    // Pour le clouage
    public int requiredTaps = 2;
    public float tapRadius = 0.2f;
    public float tapTimeout = 1.0f;

    // Variables ajoutées pour l'accès à la caméra et à la profondeur (z)
    private Camera mainCamera;
    private float zCoord;

    private bool isCovering = false;
    private bool isFixed = false;

    // Logique de Clouage
    private int leftTaps = 0;
    private float leftLastTapTime = 0f;
    private int rightTaps = 0;
    private float rightLastTapTime = 0f;

    public Vector3 leftEndLocalPos = new Vector3(-0.5f, 0, 0);
    public Vector3 rightEndLocalPos = new Vector3(0.5f, 0, 0);

    void Start()
    {
        mainCamera = Camera.main;
        // On définit le plan Z pour la conversion ScreenToWorld
        zCoord = mainCamera.WorldToScreenPoint(transform.position).z;
    }

    void Update()
    {
        if (isFixed) return;

        if (isCovering)
        {
            CheckTaps();
        }

        if (leftTaps >= requiredTaps && rightTaps >= requiredTaps)
        {
            FixPlank();
        }

        if (Time.time > leftLastTapTime + tapTimeout) leftTaps = 0;
        if (Time.time > rightLastTapTime + tapTimeout) rightTaps = 0;
    }

    private void CheckTaps()
    {
        Vector3 leftEndWorldPos = transform.TransformPoint(leftEndLocalPos);
        Vector3 rightEndWorldPos = transform.TransformPoint(rightEndLocalPos);

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Ended && touch.tapCount == 1)
            {
                // Utilisation de mainCamera et zCoord, maintenant disponibles
                Vector3 touchWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, zCoord));

                if (Vector3.Distance(touchWorldPos, leftEndWorldPos) < tapRadius)
                {
                    if (Time.time < leftLastTapTime + tapTimeout)
                    {
                        leftTaps++;
                        Debug.Log($"Tapotement Gauche détecté. Total: {leftTaps}/{requiredTaps}");
                    }
                    else
                    {
                        leftTaps = 1;
                    }
                    leftLastTapTime = Time.time;
                }
                else if (Vector3.Distance(touchWorldPos, rightEndWorldPos) < tapRadius)
                {
                    if (Time.time < rightLastTapTime + tapTimeout)
                    {
                        rightTaps++;
                        Debug.Log($"Tapotement Droite détecté. Total: {rightTaps}/{requiredTaps}");
                    }
                    else
                    {
                        rightTaps = 1;
                    }
                    rightLastTapTime = Time.time;
                }
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
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == holeObject.GetComponent<Collider>().gameObject)
        {
            isCovering = true;
            Debug.Log("Planche au-dessus du trou.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == holeObject.GetComponent<Collider>().gameObject)
        {
            isCovering = false;
            Debug.Log("Planche éloignée du trou. Compteur de clouage réinitialisé.");
            leftTaps = 0;
            rightTaps = 0;
            leftLastTapTime = 0f;
            rightLastTapTime = 0f;
        }
    }
}