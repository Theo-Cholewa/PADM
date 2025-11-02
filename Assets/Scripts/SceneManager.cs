using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Quitter l'application avec Échap
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            // si on est dans l'éditeur, arrête le Play mode
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // Raccourcis pour changer de scène : 0 -> Welcome, 1 -> RessourceTime, 2 -> Guerre
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            if (activeScene != "Welcome")
                UnityEngine.SceneManagement.SceneManager.LoadScene("Welcome");
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (activeScene != "RessourceTime")
                UnityEngine.SceneManagement.SceneManager.LoadScene("RessourceTime");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (activeScene != "Guerre")
                UnityEngine.SceneManagement.SceneManager.LoadScene("Guerre");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (activeScene != "Réparation")
                UnityEngine.SceneManagement.SceneManager.LoadScene("Réparation");
        }
    }
}
