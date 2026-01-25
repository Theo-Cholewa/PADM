using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PositionSelection : MonoBehaviour
{

    public PartyTools.ValueClient<Vector2Int> UsedRoles;
    public string NextScene;
    public UnityEvent onChange = new();

    void Start()
    {
        UsedRoles = new PartyTools.ValueClient<Vector2Int>(Party.current, $"placement", it=>JsonUtility.FromJson<Vector2Int>(it));
        UsedRoles.onChange = (p,v)=>onChange.Invoke();
    }

    void OnDestroy()
    {
        UsedRoles.Dispose();
    }

    public void Select(Vector2Int position)
    {
        if(UsedRoles.GetValues().Any(it=>it.Value==position))return;
        PlacementInfo.current.Position = position;
        UnityEngine.SceneManagement.SceneManager.LoadScene(NextScene);
    }

}
