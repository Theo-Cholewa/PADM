using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    private Vector3 target = Vector2.zero;
    private Vector3 velocity = Vector3.zero;
    private bool dragging = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (dragging){
            var direction = (target - transform.position).normalized;
            direction.z = 0;
            velocity += direction*.1f;
        }
        transform.position += velocity;
        velocity *= 0.98f;
        if (velocity.magnitude < 0.01) velocity = Vector3.zero;
    }

    void OnMouseDrag()
    {
        // Raycast mouse to z=0 plane
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var plane = new Plane(Vector3.forward, transform.position);
        if (plane.Raycast(ray, out float distance))
        {
            target = ray.GetPoint(distance);
            dragging = true;
        }
    }

    void OnMouseUp()
    {
        dragging = false;
    }
}
