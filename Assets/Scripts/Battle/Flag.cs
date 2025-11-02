using System;
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

    private HashSet<Pirate> selectedPirates = new();
    private int gestSize = 0;


    void OnGestStart(Gestable gestable)
    {
    }

    void OnGestEnd(Gestable gestable)
    {
        gestSize = gestable.gestPointList.Count;

        if (gestSize == 0)
        {
            foreach (var pirate in pirates)
            {
                if (pirate.IsDestroyed()) continue;
                pirate.SetHighlight(false);
            }
            selectedPirates.Clear();
        }

    }

    void OnGestUpdate(Gestable gestable)
    {
        var points = gestable.GetOrderedGestPoints().ToList();

        // Select the pirate pointed by the gesture
        if (points.Count == 1)
        {
            var direction = (points[0].position - transform.position).normalized;
            foreach (var pirate in pirates)
            {
                if (pirate.IsDestroyed()) continue;
                var distance = (transform.position - pirate.transform.position).magnitude;
                var nearest_point = transform.position + direction * distance;
                var distance_to_pirate = (nearest_point - pirate.transform.position).magnitude;
                if (distance_to_pirate < 10f)
                {
                    if(selectedPirates.Add(pirate))
                    {
                        pirate.SetHighlight(true);
                    }
                }
            }
        }
        // Move all pirates
        else if (points.Count == 2)
        {
            var direction = points[0].position - transform.position;
            foreach (var pirate in selectedPirates)
            {
                if (pirate.IsDestroyed()) continue;
                pirate.Move(direction);
            }
        }
        // Retrieve all pirates
        else if (points.Count == 3)
        {
            foreach (var pirate in selectedPirates)
            {
                if (pirate.IsDestroyed()) continue;
                if((transform.position - pirate.transform.position).magnitude > HomeSize)
                {
                    pirate.MoveTo(transform.position);
                }
            }
        }
    }


}
