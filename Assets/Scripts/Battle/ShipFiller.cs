using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Linq;

public class ShipFiller : MonoBehaviour
{
    private Ship ship;

    void Start()
    {
        var pos = transform.position;
        this.ship = gameObject.scene.GetRootGameObjects()
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
