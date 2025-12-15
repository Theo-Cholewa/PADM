using UnityEngine;

public class tuto : MonoBehaviour
{
    public GameObject tutoP;
    public GameObject tutoB;

    void Start()
    {

        if (tutoP != null)
        {
            tutoP.SetActive(false);
        }

        if (tutoB != null)
        {
            tutoB.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && tutoP != null)
        {
            tutoP.SetActive(!tutoP.activeSelf);


        }

        if (Input.GetKeyDown(KeyCode.B) && tutoB != null)
        {
            tutoB.SetActive(!tutoB.activeSelf);


        }
    }
}