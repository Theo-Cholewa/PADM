using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Représente un objet physique attrapable et draggable
/// </summary>
[RequireComponent(typeof(Physic))]
public class Grabbable : MonoBehaviour
{
    private Vector3 target = Vector2.zero;
    private bool dragging = false;

    public Action onTake = null;

    private Physic physic;

    // Start is called before the first frame update
    void Start()
    {
        physic = GetComponent<Physic>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (dragging)
        {
            var direction = (target - transform.position).normalized;
            direction.z = 0;
            physic.velocity += direction * .1f;
        }
    }

    void OnMouseDrag()
    {
        // Raycast mouse to z=0 plane
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var plane = new Plane(Vector3.forward, transform.position);
        if (plane.Raycast(ray, out float distance))
        {
            target = ray.GetPoint(distance);
            if (!dragging && onTake != null) onTake();
            dragging = true;
        }
    }

    void OnMouseUp()
    {
        dragging = false;
    }
}
