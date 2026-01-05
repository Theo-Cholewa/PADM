using System.Collections;
using System.Linq;
using System.Transactions;
using UnityEngine;

public class ShipFiller : MonoBehaviour
{
    public float SinkRate = 0.5f;

    public int SinkDamage = 1;

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
        
        StartCoroutine(Sink());
    }

    IEnumerator Sink()
    {
        while (true)
        {
            yield return new WaitForSeconds(SinkRate);
            ship.ChangeHealth(-SinkDamage);
        }
    }
}
