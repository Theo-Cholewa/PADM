using System.Linq;
using UnityEngine;

public class LevelDependant : MonoBehaviour
{
    public TeamEnum TeamId;
    public RessourceType Type;
    public int Minimum;
    public int Maximum;

    void Start()
    {
        var TeamClient = RessourceClient.current.Get(Team.Of(TeamId));
        var values = TeamClient.client.GetValues();
        var level = values.Count==0 ? 4 : values.First().Value.Get(Type);

        if(level<Minimum || level>Maximum) Destroy(gameObject);
    }
}
