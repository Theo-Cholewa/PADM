using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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

    void OnTouchDrag(TouchInfo info)
    {
        Debug.Log("Dragging "+info.fingerId+" "+info.position.ToString());
        // Raycast mouse to z=0 plane
        var ray = Camera.main.ScreenPointToRay(info.position);
        var plane = new Plane(Vector3.forward, transform.position);
        if (plane.Raycast(ray, out float distance))
        {
            target = ray.GetPoint(distance);
            if (!dragging && onTake != null) onTake();
            dragging = true;
        }
    }

    void OnTouchDragEnd(TouchInfo info)
    {
        dragging = false;
    }
}
