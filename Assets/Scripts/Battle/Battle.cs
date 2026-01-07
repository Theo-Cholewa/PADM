using System.Collections;
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
        StartCoroutine(SyncTime());
    }

    void FixedUpdate()
    {
        // On Fight End, Change of scene
        RessourceClient.current.GoToGoodScene();

        // Timer
        RemainingTime -= Time.fixedDeltaTime;
        if (RemainingTime <= 0)
        {
            RemainingTime = 0;
            RessourceClient.current.AskForFightEnd();
        }

        var minute = (int)(RemainingTime/60);
        var seconds = (int)(RemainingTime-minute*60);
        var TextRemainingTime = $"{minute.ToString().PadLeft(2,'0')}:{seconds.ToString().PadLeft(2,'0')}";
        foreach(var mesh in TimerMeshes)
        {
            mesh.text = TextRemainingTime;
        }
    }

    IEnumerator SyncTime()
    {
        while (true)
        {
            Party.current.SendMessageToAll($"set_time;{RemainingTime}");
            yield return new WaitForSeconds(5);
        }
    }

}
