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
public class Gestable : MonoBehaviour
{


    public float GestPointMaxDistance = 10f;

    public GameObject DraggerPrefab = null;

    public class GestPoint
    {
        public TouchInfo info;
        public Vector3 position;
        public GameObject draggerObject;
    }

    /// <summary>
    /// Dictionnaire des mains qui attrapent l'objet.
    /// Associe l'ID de chaque doigt à sa main correspondante.
    /// </summary>
    [HideInInspector]
    public Dictionary<int, GestPoint> gestPoints = new Dictionary<int, GestPoint>();

    /// <summary>
    /// Liste des ID des doigts qui attrapent l'objet, dans l'ordre dans lequel ils ont été ajoutés.
    /// </summary>
    [HideInInspector]
    public List<int> gestPointList = new List<int>();


    /// <summary>
    /// Retourne les mains qui attrapent l'objet, dans l'ordre dans lequel elles ont été ajoutées.
    /// </summary>
    /// <returns>Les mains qui attrapent l'objet.</returns>
    public IEnumerable<GestPoint> GetOrderedGestPoints()
    {
        return gestPointList.Select(fingerId => gestPoints[fingerId]);
    }

    void UpdateShape(GestPoint pulling)
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

    Vector3 GetCoord(Vector2 coord)
    {
        var ray = Camera.main.ScreenPointToRay(coord);
        var plane = new Plane(Vector3.forward, transform.position);
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        throw new Exception("Could not get coordinate");
    }

    // Pressing a finger down on the object start the "Gest Mode"
    private bool gestMode = false;

    void OnTouchDown(TouchInfo info)
    {
        if (!gestMode) transform.localScale *= 1.1f;
        gestMode = true;
    }

    void OnTouchDragEnd(TouchInfo info)
    {
        if (gestMode) transform.localScale /= 1.1f;
        gestMode = false;
    }

    void OnGlobalTouchDown(GlobalTouchInfo global)
    {
        var info = global.info;
        var touchPos = GetCoord(info.position);
        Debug.Log((transform.position - touchPos).magnitude);
        if (gestMode && (transform.position - touchPos).magnitude < GestPointMaxDistance)
        {
            var point = new GestPoint
            {
                draggerObject = DraggerPrefab ? Instantiate(DraggerPrefab) : null,
                info = info,
                position = GetCoord(info.position)
            };
            gestPoints[info.fingerId] = point;
            gestPointList.Add(info.fingerId);
            if (point.draggerObject) UpdateShape(point);
            global.doCancel = true;
            SendMessage("OnGestStart", this, SendMessageOptions.DontRequireReceiver);
        }
    }

    void OnGlobalTouchUp(GlobalTouchInfo global)
    {
        if (gestPoints.TryGetValue(global.info.fingerId, out var point))
        {
            gestPoints.Remove(global.info.fingerId);
            gestPointList.Remove(global.info.fingerId);
            Destroy(point.draggerObject);
            global.doCancel = true;
            SendMessage("OnGestEnd", this, SendMessageOptions.DontRequireReceiver);
        }
    }
    
    void FixedUpdate()
    {
        for (int i = 0; i < gestPointList.Count; i++)
        {
            var fingerId = gestPointList[i];
            if (gestPoints.TryGetValue(fingerId, out var point))
            {
                point.position = GetCoord(point.info.position);
                UpdateShape(point);
            }
        }
        if (gestPointList.Count > 0)
        {
            SendMessage("OnGestUpdate", this, SendMessageOptions.DontRequireReceiver);
        }
    }

}
