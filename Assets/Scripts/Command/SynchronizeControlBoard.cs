using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SynchronizeControlBoard : MonoBehaviour
{
    public Helm helm;
    public Lever lever;
    public MultiTouchButton button;

    private bool isAnchored = false;

    PartyTools.ValueServer<(float,float,float)> server;

    void Start()
    {
        var party = Party.current;
        server = new(party, $"direction_{Team.currentTeam.id}", (0f, 0f, 0f), v=>JsonUtility.ToJson(v));
        StartCoroutine(SendData());
        button.onTouchDown.AddListener(SwitchAnchor);
    }


    void OnDestroy()
    {
        server.Dispose();
    }

    IEnumerator SendData()
    {
        while (true)
        {
            var rotation = Math.Clamp(helm.rotation/180f, -1f, 1f);
            if(server!=null)server.SetValue((rotation, lever.signal, isAnchored ? 1f : 0f));
            Debug.Log($"Sending data: rotation={rotation}, speed={lever.signal}, anchored={isAnchored}");
            yield return new WaitForSeconds(0.1f);
        }
    }

    void SwitchAnchor()
    {
        isAnchored = !isAnchored;
    }
}
