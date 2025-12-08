using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour
{
    public Transform FinalPosition;

    [HideInInspector]
    public float signal;

    Vector3 initialPosition;
    Vector3 finalPosition;

    void Start()
    {
        initialPosition = transform.position;
        finalPosition = FinalPosition.position;
    }


    void OnTouchDrag(TouchInfo info)
    {
        var ray = Camera.main.ScreenPointToRay(info.position);
        new Plane(Vector3.forward, transform.position).Raycast(ray, out var distance);
        var newPos = ray.GetPoint(distance);

        var start_to_end = finalPosition-initialPosition;
        var start_to_point = newPos - initialPosition;
        var advancement = Vector3.Dot(start_to_point, start_to_end) / start_to_end.sqrMagnitude;
        this.signal = Math.Max(0,Math.Min(1,advancement));
        Debug.Log(this.signal);
        transform.position = Vector3.Lerp(initialPosition, finalPosition, this.signal);
    }
}
