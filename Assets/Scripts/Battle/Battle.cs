using System.Collections.Generic;
using UnityEngine;

public class Battle : MonoBehaviour
{
    public List<TextMesh> TimerMeshes;
    public int BattleDuration = 60 * 2;

    private float RemainingTime = 0;

    void Start()
    {
        RemainingTime = BattleDuration;
    }

    void FixedUpdate()
    {
        // On Fight End, Change of scene
        var stats = RessourceClient.current.GameStats.FirstOrDefault();
        if(!stats.IsInFight)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("RessourceTime");
        }

        // Timer
        RemainingTime -= Time.fixedDeltaTime;
        if (RemainingTime <= 0)
        {
            RemainingTime = 0;
            RessourceClient.current.AskForFightEnd();
        }

        var minute = (int)(RemainingTime/60);
        var seconds = (int)(RemainingTime-minute);
        var TextRemainingTime = $"{minute.ToString().PadLeft(2,'0')}:{seconds.ToString().PadLeft(2,'0')}";
        foreach(var mesh in TimerMeshes)
        {
            mesh.text = TextRemainingTime;
        }
    }

}
