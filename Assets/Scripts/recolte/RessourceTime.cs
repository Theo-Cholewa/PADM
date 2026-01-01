using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RessourceTime : MonoBehaviour
{
    void FixedUpdate()
    {
        // Change scene if fight start or win
        var stats = RessourceClient.current.FightStats.FirstOrDefault();
        if(stats.IsInFight)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Battle");
        }
    }
}
