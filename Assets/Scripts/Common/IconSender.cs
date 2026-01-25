
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class IconSender : MonoBehaviour
{
    public static IconSender current;


    public string Name;
    
    public RectTransform Zone;

    public UDictionary<string,IconSenderIcon> Icons;

    [HideInInspector] public Dictionary<string,IconTarget> Targets = new();


    [HideInInspector] public PlacementInfo placement;

    void Start()
    {
        placement = PlacementInfo.current;
        Party.current.OnMessage.AddListener(OnMessage);
        current = this;
    }

    void OnDestroy()
    {
        Party.current?.OnMessage.RemoveListener(OnMessage);
        if(current==this)current=null;
    }

    public Vector2 LocalToWorld(Vector2 pos) => placement.Position + pos;
    public Vector2 WorldToLocal(Vector2 pos) => pos - placement.Position;

    public Vector2 SceneToWorld(Vector3 pos) => LocalToWorld(SceneToLocal(pos));

    public Vector3 WorldToScene(Vector2 pos) => LocalToScene(WorldToLocal(pos));

    public Vector2 SceneToLocal(Vector3 pos)
    {
        // Récupérer les coins du RectTransform
        Vector3[] corners = new Vector3[4];
        Zone.GetWorldCorners(corners);

        Vector3 bottomLeft = corners[0];
        Vector3 horizontal = corners[3] - corners[0];
        Vector3 vertical = corners[1] - corners[0];

        // Calculer les coordonnées locales normalisées
        Vector3 delta = pos - bottomLeft;

        float x = Vector3.Dot(delta, horizontal) / horizontal.sqrMagnitude;
        float y = Vector3.Dot(delta, vertical) / vertical.sqrMagnitude;

        return new Vector2(x, y);
    }

    public Vector3 LocalToScene(Vector2 pos)
    {
        // Get rect transform
        Vector3[] corners = new Vector3[4];
        Zone.GetWorldCorners(corners);

        Vector3 initial = corners[0];
        Vector3 x = corners[3]-corners[0];
        Vector3 y = corners[1]-corners[0];

        return initial + x * pos.x + y * pos.y;
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
        SpawnIconLocally(icon,from,to);
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
    public void SpawnIcon(string icon, Vector2 from, string to, bool isReversed=false)
    {
        Debug.Log(Targets.Keys.ToList().ToCommaSeparatedString());

        // Try locally
        if (Targets.TryGetValue(to, out var target))
        {
            if(isReversed) SpawnIcon(icon, target.GetWorldPosition(), from);
            else SpawnIcon(icon, from, target.GetWorldPosition());
        }

        // Try online
        var msg = $"icon_sender_propose;{JsonUtility.ToJson((icon, from, to, isReversed))}";
        Party.current.SendMessageToAll(msg);
    }

    void OnMessage(PartyMessage packet)
    {
        var msg = packet.message;
        if (msg.StartsWith("icon_sender_summon;"))
        {
            Debug.Log("aaaaaaaaaa");
            var (id,from,to) = JsonUtility.FromJson<(string,Vector3,Vector3)>(msg.Substring("icon_sender_summon;".Length));
            SpawnIconLocally(id, from, to);
        }
    }

}
