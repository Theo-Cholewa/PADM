using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [Header("Scene to load name")]
    public string sceneName = "RessourceTime";

    [Header("Key to press to switch scene")]
    public string keyToPress = "P";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) 
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
