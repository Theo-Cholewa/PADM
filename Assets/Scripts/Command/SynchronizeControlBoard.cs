using System.Collections;
using UnityEngine;

public class SynchronizeControlBoard : MonoBehaviour
{
    public Helm helm;
    public Lever lever;

    PartyTools.ValueServer<(float,float)> server;

    void Start()
    {
        var party = Party.current;
        server = new(party,$"direction_{Team.currentTeam.id}", (0f, 0f), v=>JsonUtility.ToJson(v));
        StartCoroutine(SendData());
    }


    void OnDestroy()
    {
        server.Dispose();
    }

    IEnumerator SendData()
    {
        while (true)
        {
            if(server!=null)server.SetValue((helm.rotation, lever.signal));
            Debug.Log($"Sending data: rotation={helm.rotation}, speed={lever.signal}");
            yield return new WaitForSeconds(0.1f);
        }
    }
}
