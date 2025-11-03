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

    [Header("Contrôleur de nuages (pour Welcome uniquement)")]
    public CloudRevealController cloudController;

    private bool isSwitching = false;

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

        // Raccourcis pour changer de scène : 1 -> Welcome, 2 -> RessourceTime, 3 -> RessourceBois, 4 -> Guerre, 5 -> Réparation
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (Input.anyKeyDown)
        {
            for (int i = 0; i < scenes.Count; i++)
            {
                if (i < keyCodes.Count && Input.GetKeyDown(keyCodes[i]))
                {
                    string targetScene = scenes[i];
                    if (activeScene != targetScene)
                    {
                        if (isSwitching) return;
                        StartCoroutine(SwitchSceneWithClouds(activeScene, targetScene));
                        break;
                    }
                }
            }
        }
    }

    private IEnumerator SwitchSceneWithClouds(string currentScene, string nextScene)
    {
        isSwitching = true;

        // Jouer l'animation de nuages pour masquer la transition
        if (cloudController != null)
        {
            yield return StartCoroutine(cloudController.PlayCloudHide());
            yield return new WaitForSeconds(0.2f);
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        isSwitching = false;
    }
}
