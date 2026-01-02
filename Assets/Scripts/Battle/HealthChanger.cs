using System.Linq;
using UnityEngine;

public class HealthChanger : MonoBehaviour
{
    public int onSpawn = 0;
    public int onDestroy = 0;
    private Ship ship;

    void Start()
    {
        var ship = gameObject.scene.GetRootGameObjects()
            .Select(go =>
            {
                if (go.TryGetComponent(out Ship ship)) return ship;
                else return null;
            })
            .Where(ship => ship != null)
            .OrderBy(ship => (ship.transform.position - transform.position).magnitude)
            .First();
        
        if(onSpawn!=0) ship.ChangeHealth(onSpawn);
    }

    void OnDestroy()
    {
        if(onDestroy!=0) ship.ChangeHealth(onDestroy);
    }
}
