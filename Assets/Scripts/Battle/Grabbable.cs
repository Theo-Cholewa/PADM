using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Timeline;

/// <summary>
/// Représente un objet physique attrapable et draggable
/// </summary>
[RequireComponent(typeof(Solid))]
public class Grabbable : MonoBehaviour
{

    public class GrabHand
    {
        public int fingerId;
        public Vector3 position;
        public GameObject draggerObject;
    }

    /// <summary>
    /// Dictionnaire des mains qui attrapent l'objet.
    /// Associe l'ID de chaque doigt à sa main correspondante.
    /// </summary>
    [HideInInspector]
    public Dictionary<int, GrabHand> grabHands = new Dictionary<int, GrabHand>();

    /// <summary>
    /// Liste des ID des doigts qui attrapent l'objet, dans l'ordre dans lequel ils ont été ajoutés.
    /// </summary>
    [HideInInspector]
    public List<int> grabHandList = new List<int>();

    public GameObject DraggerPrefab = null;

    /// <summary>
    /// Retourne les mains qui attrapent l'objet, dans l'ordre dans lequel elles ont été ajoutées.
    /// </summary>
    /// <returns>Les mains qui attrapent l'objet.</returns>
    public IEnumerable<GrabHand> GetOrderedGrabHands()
    {
        return grabHandList.Select(fingerId => grabHands[fingerId]);
    }

    void FixedUpdate()
    {
        if(grabHandList.Count > 0)
        {
            SendMessage("OnGrabUpdate", this, SendMessageOptions.DontRequireReceiver);
        }
    }

    void UpdateShape(GrabHand pulling)
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

    void UpdateTarget(GrabHand pulling, TouchInfo info)
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
        var pulling = new GrabHand
        {
            fingerId = info.fingerId,
            position = info.position,
            draggerObject = DraggerPrefab ? Instantiate(DraggerPrefab) : null
        };
        UpdateTarget(pulling, info);
        if (pulling.draggerObject != null) UpdateShape(pulling);

        grabHands[info.fingerId] = pulling;
        grabHandList.Add(info.fingerId);

        SendMessage("OnGrabStart", this, SendMessageOptions.DontRequireReceiver);
    }

    void OnTouchDrag(TouchInfo info)
    {
        var pulling = grabHands[info.fingerId];
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
        var pulling = grabHands[info.fingerId];
        if (pulling != null)
        {
            grabHands.Remove(info.fingerId);
            grabHandList.Remove(info.fingerId);
            if (pulling.draggerObject != null) Destroy(pulling.draggerObject);
            SendMessage("OnGrabEnd", this, SendMessageOptions.DontRequireReceiver);
        }
    }
}
