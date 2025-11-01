using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Représente un objet physique attrapable et draggable
/// </summary>
[RequireComponent(typeof(Solid))]
public class Grabbable : MonoBehaviour
{

    public class Pulling
    {
        public int fingerId;
        public Vector3 position;
        public GameObject draggerObject;
    }

    public Dictionary<int, Pulling> pullings = new Dictionary<int, Pulling>();

    public List<int> fingers = new List<int>();

    public GameObject DraggerPrefab = null;

    public IEnumerable<Pulling> GetPullings()
    {
        return fingers.Select(fingerId => pullings[fingerId]);
    }

    void FixedUpdate()
    {
        if(fingers.Count > 0)
        {
            SendMessage("OnPullingUpdate", this, SendMessageOptions.DontRequireReceiver);
        }
    }

    void UpdateShape(Pulling pulling)
    {
        var from = pulling.position;
        var to = transform.position;
        var center = (from + to) / 2;
        pulling.draggerObject.transform.parent = null;
        pulling.draggerObject.transform.localPosition = center;
        pulling.draggerObject.transform.localScale = new Vector3((from - to).magnitude, 5f, 1f);
        pulling.draggerObject.transform.localRotation = Quaternion.FromToRotation(Vector3.left, (to - from).normalized);
        pulling.draggerObject.transform.parent = transform;
    }

    void UpdateTarget(Pulling pulling, TouchInfo info)
    {
        var ray = Camera.main.ScreenPointToRay(info.position);
        var plane = new Plane(Vector3.forward, transform.position);
        if (plane.Raycast(ray, out float distance))
        {
            pulling.position = ray.GetPoint(distance);
        }
    }

    void OnTouchDown(TouchInfo info)
    {
        Debug.Log("Touch down "+info.fingerId);
        var pulling = new Pulling
        {
            fingerId = info.fingerId,
            position = info.position,
            draggerObject = DraggerPrefab ? Instantiate(DraggerPrefab) : null
        };
        UpdateTarget(pulling, info);
        if (pulling.draggerObject != null) UpdateShape(pulling);

        pullings[info.fingerId] = pulling;
        fingers.Add(info.fingerId);

        SendMessage("OnPullingStart", this, SendMessageOptions.DontRequireReceiver);
    }

    void OnTouchDrag(TouchInfo info)
    {
        var pulling = pullings[info.fingerId];
        if (pulling != null)
        {
            // Visual
            if (pulling.draggerObject != null) UpdateShape(pulling);

            // Update pulling position
            UpdateTarget(pulling, info);
        }
    }

    void OnTouchDragEnd(TouchInfo info)
    {
        Debug.Log("Touch drag end " + info.fingerId);
        var pulling = pullings[info.fingerId];
        if (pulling != null)
        {
            pullings.Remove(info.fingerId);
            fingers.Remove(info.fingerId);
            if (pulling.draggerObject != null) Destroy(pulling.draggerObject);
            SendMessage("OnPullingEnd", this, SendMessageOptions.DontRequireReceiver);
        }
    }
}
