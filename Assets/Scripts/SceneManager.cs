using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneManager : MonoBehaviour
{

    private static List<KeyCode> keyCodes = new List<KeyCode>{
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3,
        KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7,
        KeyCode.Alpha8, KeyCode.Alpha9,
    };

    public List<string> scenes = new List<string>();

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

        if (Input.anyKeyDown)
        {
            for (int i = 0; i < scenes.Count; i++)
            {
                if (Input.GetKeyDown(keyCodes[i]))
                {
                    if(activeScene != scenes[i]) UnityEngine.SceneManagement.SceneManager.LoadScene(scenes[i]);
                }
            }
        }
    }
}
