using UnityEngine;

public class RessourceTime : MonoBehaviour
{
    void FixedUpdate()
    {
                    Debug.Log(JsonUtility.ToJson(RessourceClient.current.GameStats.GetValues()));


        // Change scene if fight start or win
        var stats = RessourceClient.current.GameStats.FirstOrDefault();
        if(stats.IsInFight)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Battle");
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.B))
        {
            Debug.Log("ASk for fight");
            RessourceClient.current.Get(Team.RED).AskForFight();
        }
    }
}
