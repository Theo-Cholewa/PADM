using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public List<Text> Texts;

    private float currentTime = 0;
    

    void Update()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;

            var minutes = (int)(currentTime / 60);
            var secondes = (int)(currentTime-minutes*60);
            foreach(var Text in Texts) Text.text = $"{minutes:00}:{secondes:00}";
        }
        else
        {
            foreach(var Text in Texts) Text.text = "00:00";
        }
    }

    void Start()
    {
        Party.current.OnMessage.AddListener(OnMessage);
    }

    void OnDestroy()
    {
        Party.current?.OnMessage?.RemoveListener(OnMessage);
    }

    void OnMessage(PartyMessage msg)
    {
        if (msg.message.StartsWith("set_time;"))
        {
            currentTime = float.Parse(msg.message.Substring("set_time;".Length));
        }
    }

}
