
using UnityEngine;

public class IconSender : MonoBehaviour
{
    public string Name;

    public UDictionary<string,Vector2> Targets;
    
    public RectTransform Zone;

    public UDictionary<string,IconSenderIcon> Icons;

    [HideInInspector] public PlacementInfo placement;

    void Start()
    {
        placement = PlacementInfo.current;
        Party.current.OnMessage.AddListener(OnMessage);
    }

    void OnDestroy()
    {
        Party.current?.OnMessage.RemoveListener(OnMessage);
    }

    /// <summary>
    /// Affiche une icone sur cette instance qui se déplace d'un point à un autre.
    /// </summary>
    /// <param name="icon">le type d'icône</param>
    /// <param name="from">la position de départ</param>
    /// <param name="to">la position d'arrivée</param>
    public void SpawnIconLocally(string icon, Vector2 from, Vector2 to)
    {
        var prefab = Icons[icon];
        if(prefab==null) return;

        var iconInstance = Instantiate(prefab,Zone);

        iconInstance.sender = this;
        iconInstance.worldPosition = from;
        iconInstance.targetPosition = to;
        iconInstance.sender = this;

        iconInstance.gameObject.SetActive(true);
    }

    /// <summary>
    /// Affiche une icone sur toutes les instances qui se déplace d'un point à un autre.
    /// </summary>
    /// <param name="icon">le type d'icône</param>
    /// <param name="from">la position de départ</param>
    /// <param name="to">la position d'arrivée</param>
    public void SpawnIcon(string icon, Vector2 from, Vector2 to)
    {
        var msg = $"icon_sender_summon;{JsonUtility.ToJson((icon, from, to))}";
        Party.current.SendMessageToAll(msg);
    }

    /// <summary>
    /// Affiche une icone sur toutes les instances qui se déplace d'un point à un autre.
    /// La coordonnée d'arrivée n'est pas connu, on demande à toute la Party si quelqu'un connait la coordonnée
    /// de ce point.
    /// </summary>
    /// <param name="icon">le type d'icône</param>
    /// <param name="from">la position de départ</param>
    /// <param name="to">la position d'arrivée</param>
    /// <param name="to2">une autre position d'arrivée</param>
    public void SpawnIcon(string icon, Vector2 from, string to, string to2)
    {
        // Try locally
        if (Name == to && Targets.TryGetValue(to2, out var target))
        {
            SpawnIcon(icon, from, placement.Position + target);
        }

        // Try online
        var msg = $"icon_sender_propose;{JsonUtility.ToJson((icon, from, to, to2))}";
        Party.current.SendMessageToAll(msg);
    }

    void OnMessage(PartyMessage packet)
    {
        var msg = packet.message;
        if (msg.StartsWith("icon_sender_summon;"))
        {
            var (id,from,to) = JsonUtility.FromJson<(string,Vector3,Vector3)>(msg.Substring("icon_sender_summon;".Length));
            SpawnIconLocally(id, from, to);
        }
        else if (msg.StartsWith("icon_sender_propose;"))
        {
            var (id,from,name,name2) = JsonUtility.FromJson<(string,Vector3,string,string)>(msg.Substring("icon_sender_propose;".Length));
            if (Name == name && Targets.TryGetValue(name2, out var target))
            {
                SpawnIcon(id, from, placement.Position + target);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            SpawnIconLocally("canon_ball", Vector2.zero, Vector2.one*100);
        }
    }

}
