using UnityEngine;

public class GameManagerReparation : MonoBehaviour
{
    public GameObject troueBateau1;
    public GameObject troueBateau2;
    public GameObject[] planchesBateau1;
    public GameObject[] planchesBateau2;

    private GameObject selectedPlanche;
    private int plancheIndexBateau1 = 0;
    private int plancheIndexBateau2 = 0;

    void Start()
    {
        troueBateau1.SetActive(false);
        troueBateau2.SetActive(false);
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (selectedPlanche != null)
            {
                foreach (GameObject planche in planchesBateau1)
                {
                    if (selectedPlanche == planche)
                    {
                        troueBateau1.SetActive(false);
                        selectedPlanche.SetActive(false);
                        selectedPlanche = null;
                        return;
                    }
                }
            }
            troueBateau1.SetActive(!troueBateau1.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (selectedPlanche != null)
            {
                foreach (GameObject planche in planchesBateau2)
                {
                    if (selectedPlanche == planche)
                    {
                        troueBateau2.SetActive(false);
                        selectedPlanche.SetActive(false);
                        selectedPlanche = null;
                        return;
                    }
                }
            }
            troueBateau2.SetActive(!troueBateau2.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            SelectPlanche(planchesBateau1, ref plancheIndexBateau1);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
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

        index = (index + 1) % planches.Length;
    }
}