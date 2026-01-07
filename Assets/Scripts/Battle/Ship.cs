using System.Linq;
using UnityEngine;
public class Ship : MonoBehaviour
{
    public TeamEnum TeamId;
    public new Renderer renderer;

    public RessourceClient.TeamClient ressources;
    

    void Awake()
    {
        foreach(var level in gameObject.GetComponentsInChildren<LevelDependant>())
            level.TeamId = TeamId;

        foreach(var flag in gameObject.GetComponentsInChildren<Flag>())
            flag.TeamId = TeamId;

        foreach(var flag in gameObject.GetComponentsInChildren<Canon>())
            flag.team = TeamId;

        foreach(var box in gameObject.GetComponentsInChildren<Box>())
            box.TeamId = TeamId;
    }

    void Start()
    {
        ressources = RessourceClient.current.Get(Team.Of(TeamId));
        ressources.onChange.AddListener(OnHealthChange);
    }

    void OnDestroy()
    {
        ressources.onChange.RemoveListener(OnHealthChange);
    }

    public void ChangeHealth(int offset)
    {
        ressources.Add(RessourceType.Health, offset);
    }

    void OnHealthChange()
    {
        var values = ressources.client.GetValues().ToList();
        var health = values.Count>0 ? values[0].Value.health : 100;
        Color c = renderer.material.color;
        c.a = health/100f;
        renderer.material.color = c;
    }
}
