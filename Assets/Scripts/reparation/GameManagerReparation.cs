using UnityEngine;
using System.Collections;

public class GameManagerReparation : MonoBehaviour
{
    // Public variables
    public GameObject troueBateau1;
    public GameObject troueBateau2;
    public GameObject[] planchesBateau1;
    public GameObject[] planchesBateau2;
    public GameObject eau1;
    public GameObject eau2;
    public GameObject Sceau1;
    public GameObject Sceau2;

    // Private variables
    private GameObject selectedPlanche;
    private int plancheIndexBateau1 = 0;
    private int plancheIndexBateau2 = 0;
    private float troue1Timer = 0f;
    private float troue2Timer = 0f;
    private const float waterDelay = 10f;

    void Start()
    {
        troueBateau1.SetActive(false);
        troueBateau2.SetActive(false);
        eau1.SetActive(false);
        eau2.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Touche 'Q' pressée. Tentative de gestion du trou du Bateau 1.");
            if (selectedPlanche != null)
            {
                foreach (GameObject planche in planchesBateau1)
                {
                    if (selectedPlanche == planche)
                    {
                        troueBateau1.SetActive(false);
                        selectedPlanche.SetActive(false);
                        selectedPlanche = null;
                        troue1Timer = 0f;
                        eau1.SetActive(false);
                        Debug.Log("Planche du Bateau 1 utilisée. Le trou et la planche ont disparu.");
                        return;
                    }
                }
            }

            // Le trou est basculé seulement si l'action de réparation ne se produit pas
            troueBateau1.SetActive(!troueBateau1.activeSelf);
            if (troueBateau1.activeSelf)
            {
                Debug.Log("Le trou du Bateau 1 est apparu.");
                troue1Timer = 0f;
            }
            else
            {
                Debug.Log("Le trou du Bateau 1 a été caché.");
                troue1Timer = 0f;
                eau1.SetActive(false);
            }
        }

        // Handle troueBateau2 logic
        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("Touche 'W' pressée. Tentative de gestion du trou du Bateau 2.");
            if (selectedPlanche != null)
            {
                foreach (GameObject planche in planchesBateau2)
                {
                    if (selectedPlanche == planche)
                    {
                        troueBateau2.SetActive(false);
                        selectedPlanche.SetActive(false);
                        selectedPlanche = null;
                        troue2Timer = 0f;
                        eau2.SetActive(false);
                        Debug.Log("Planche du Bateau 2 utilisée. Le trou et la planche ont disparu.");
                        return;
                    }
                }
            }

            troueBateau2.SetActive(!troueBateau2.activeSelf);
            if (troueBateau2.activeSelf)
            {
                Debug.Log("Le trou du Bateau 2 est apparu.");
                troue2Timer = 0f;
            }
            else
            {
                Debug.Log("Le trou du Bateau 2 a été caché.");
                troue2Timer = 0f;
                eau2.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("Touche 'A' pressée. Tentative de vider l'eau.");
            if (eau1.activeSelf)
            {
                eau1.SetActive(false);
                Debug.Log("L'eau du Bateau 1 a été vidée.");
            }
            if (eau2.activeSelf)
            {
                eau2.SetActive(false);
                Debug.Log("L'eau du Bateau 2 a été vidée.");
            }
        }

        if (troueBateau1.activeSelf)
        {
            troue1Timer += Time.deltaTime;
            if (troue1Timer >= waterDelay)
            {
                eau1.SetActive(true);
            }
        }

        if (troueBateau2.activeSelf)
        {
            troue2Timer += Time.deltaTime;
            if (troue2Timer >= waterDelay)
            {
                eau2.SetActive(true);
            }
        }

        // Selection logic remains the same
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Touche 'E' pressée. Sélection d'une planche sur le Bateau 1.");
            SelectPlanche(planchesBateau1, ref plancheIndexBateau1);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Touche 'R' pressée. Sélection d'une planche sur le Bateau 2.");
            SelectPlanche(planchesBateau2, ref plancheIndexBateau2);
        }
    }

    private void SelectPlanche(GameObject[] planches, ref int index)
    {
        if (selectedPlanche != null)
        {
            selectedPlanche.GetComponent<Renderer>().material.color = Color.white;
        }

        selectedPlanche = planches[index];
        selectedPlanche.GetComponent<Renderer>().material.color = Color.yellow;
        Debug.Log("Planche sélectionnée : " + selectedPlanche.name);

        index = (index + 1) % planches.Length;
    }
}