using UnityEngine;

public class IconTarget : MonoBehaviour
{
    public string Name;

    public IconSender Sender;

    void Start()
    {
        Party.current.OnMessage.AddListener(OnMessage);
        Sender.Targets.Add(Name, this);
    }

    void OnDestroy()
    {
        Party.current?.OnMessage.RemoveListener(OnMessage);
        Sender.Targets.Remove(Name);
    }

    public Vector2 GetWorldPosition()
    {
        return Sender.SceneToWorld(transform.position);
    }

    void OnMessage(PartyMessage packet)
    {
        var msg = packet.message;
        if (msg.StartsWith("icon_sender_propose;"))
        {
            var parsed = JsonUtility.FromJson<(string,Vector2,string,bool)>(msg.Substring("icon_sender_propose;".Length));
            var (id,from,name,isReversed) = parsed;
            if (Name == name)
            {
                if(isReversed) Sender.SpawnIcon(id, GetWorldPosition(), from);
                else Sender.SpawnIcon(id, from, GetWorldPosition());
            }
        }
    }

}
