using UnityEngine;

public class PlacementInfo : MonoBehaviour
{
    
    public static PlacementInfo current;

    public PartyTools.ValueServer<Vector2Int> server;

    void Start()
    {
        if (current != null)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        current = this;

        server = new(Party.current, "placement", new Vector2Int(1,1), it=>JsonUtility.ToJson(it));
    }

    void OnDestroy()
    {
        if (current == this) current = null;
        server?.Dispose();
    }

    public Vector2Int Position
    {
        get => server.GetValue();
        set => server.SetValue(value);
    }

    public Vector2 GetDirectionTo(Vector2Int targetPosition)
    {
        Vector2 currentPosition = new Vector2(Position.x, Position.y);
        Vector2 targetPos = new Vector2(targetPosition.x, targetPosition.y);
        return (targetPos - currentPosition).normalized;
    }


}
