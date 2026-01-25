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
            var parsed = JsonUtility.FromJson<(string,Vector2,string)>(msg.Substring("icon_sender_propose;".Length));
            var (id,from,name) = parsed;
            if (Name == name)
            {
                Sender.SpawnIcon(id, from, GetWorldPosition());
            }
        }
    }

}
