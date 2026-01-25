using System.Linq;
using UnityEngine;
public class Ship : MonoBehaviour
{
    public TeamEnum TeamId;
    public new Renderer renderer;

    public RessourceClient.TeamClient ressources;
    

    void Awake()
    {
        foreach(var level in GetComponentsInChildren<LevelDependant>()) level.TeamId = TeamId;

        foreach(var flag in GetComponentsInChildren<Flag>()) flag.TeamId = TeamId;

        foreach(var canon in GetComponentsInChildren<Canon>()) canon.team = TeamId;

        foreach(var box in GetComponentsInChildren<Box>()) box.TeamId = TeamId;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            IconSender.current?.SpawnIcon("Wood", new Vector2(0.5f,0.5f), new Vector2(1f,1f));
        }
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
