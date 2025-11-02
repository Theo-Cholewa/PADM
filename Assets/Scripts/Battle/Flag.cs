using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Gestable))]
public class Flag : MonoBehaviour
{
    public Color color;
    public MeshRenderer Colored;
    public float HomeSize = 50f;
    

    [HideInInspector]
    public List<Pirate> pirates = new();

    void Update()
    {
        Colored.material.color = color;
    }

    void OnTouchDown(TouchInfo info)
    {
        transform.localScale *= 1.1f;
    }

    void OnTouchDragEnd(TouchInfo info)
    {
        transform.localScale /= 1.1f;
    }

    private Pirate movingPirate = null;
    private int gestSize = 0;

    void OnGestStart(Gestable gestable)
    {
        gestSize = gestable.gestPointList.Count;

        // Gest with one finger move the nearest pirate
        if (gestSize == 1)
        {
            movingPirate = pirates
                .Where(p => !p.IsDestroyed())
                .OrderBy(p => (p.transform.position - transform.position).sqrMagnitude)
                .FirstOrDefault();
        }
    }

    void OnGestEnd(Gestable gestable)
    {
        gestSize = gestable.gestPointList.Count;

        if (gestSize != 1)
        {
            movingPirate = null;
        }
    }

    void OnGestUpdate(Gestable gestable)
    {
        var points = gestable.GetOrderedGestPoints().ToList();

        // Move the single pirate
        if (points.Count == 1 && movingPirate != null)
        {
            var direction = (points[0].position - transform.position);
            movingPirate.Move(direction);
        }
        // Move all pirates
        else if (points.Count == 2)
        {
            var direction = ((points[0].position + points[1].position) / 2 - transform.position);
            foreach (var pirate in pirates)
            {
                pirate.Move(direction);
            }
        }
        // Retrieve all pirates
        else if (points.Count == 3)
        {
            foreach (var pirate in pirates)
            {
                if((transform.position - pirate.transform.position).magnitude > HomeSize)
                {
                    pirate.MoveTo(transform.position);
                }
            }
        }
    }


}
