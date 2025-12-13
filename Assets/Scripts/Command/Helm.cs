using System;
using System.Collections.Generic;
using UnityEngine;

public class Helm : MonoBehaviour
{
    [HideInInspector]
    public float rotation = 0f;

    void SetRotation(float new_rotation)
    {
        rotation = new_rotation;
        transform.rotation = Quaternion.Euler(0, 0, rotation);
    }
    
    private Dictionary<int,Vector3> last = new();


    Vector3 ToPosition(Vector2 pos)
    {
        var ray = Camera.main.ScreenPointToRay(pos);
        var plane = new Plane(Vector3.forward, transform.position);
        if (plane.Raycast(ray, out var distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    void OnTouchDown(TouchInfo info)
    {
        last[info.fingerId] = ToPosition(info.position);
        isMoving = true;
    }

    void OnTouchDragEnd(TouchInfo info)
    {
        last.Remove(info.fingerId);
        isMoving = false;
    }

    bool isMoving = false;

    void OnTouchDrag(TouchInfo info)
    {
        var lastpos = last[info.fingerId];
        var pos = ToPosition(info.position);
        last[info.fingerId] = pos;

        // Wanted direction
        var ideal_direction = transform.position - pos;
        ideal_direction.Normalize();
        var a = ideal_direction.x;
        ideal_direction.x = -ideal_direction.y;
        ideal_direction.y = a;

        // Direction
        var direction = pos - lastpos;
        var length = direction.magnitude;
        if (length > 0)
        {
            direction.Normalize();
            var power = Vector3.Dot(direction, ideal_direction) * length;
            if (Math.Abs(power) > 0.1)
            {
                SetRotation(rotation - power);
            }
        }

    }

    void FixedUpdate()
    {
        if (!isMoving)
        {
            SetRotation(Mathf.Lerp(rotation, 0, 0.02f));
        }
    }
}
