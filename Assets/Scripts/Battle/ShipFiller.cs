using System.Linq;
using UnityEngine;

public class ShipFiller : MonoBehaviour
{
    private Ship ship;

    void Start()
    {
        var pos = transform.position;
        ship = gameObject.scene.GetRootGameObjects()
            .Select(go =>
            {
                if (go.TryGetComponent(out Ship ship)) return ship;
                else return null;
            })
            .Where(ship => ship != null)
            .OrderBy(ship => (ship.transform.position - pos).magnitude)
            .First();
        ship.speed += 4f;
    }

    void OnDestroy()
    {
        ship.speed -= 4f;
    }
}
