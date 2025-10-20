using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;


public struct TouchInfo
{
    public Vector2 position;
    public int fingerId;
}

/// <summary>
/// OnTouchDown - Quand on pose un doigt sur l'objet
/// OnTouchUp - Quand on lève le doigt sur l'objet
/// OnTouchDrag - Après que le doigt ait été posé sur l'objet, appelée à chaque frame tant que le doigt est maintenu.
/// OnTouchDragEnd - Quand le doigt est levé après avoir été maintenu, après avoir été posé sur l'objet
/// </summary>
public class MultiTouchHandler : MonoBehaviour
{
    GameObject GetTarget(Vector2 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow, 100f);

        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log(hit.transform.name);
            if (hit.collider != null)
            {
                return hit.transform.gameObject;
            }
        }
        return null;
    }

    class TouchState
    {
        public GameObject obj;
        public TouchInfo info;
    }

    private Dictionary<int, TouchState> dict = new();


    void TouchBegan(TouchInfo info)
    {
        var target = GetTarget(info.position);
        if (target != null)
        {
            target.SendMessage("OnTouchDown", info, SendMessageOptions.DontRequireReceiver);
            dict[info.fingerId] = new TouchState { obj = target, info = info };
        }
    }

    void TouchEnded(TouchInfo info)
    {
        var target = GetTarget(info.position);
        if (target != null)
        {
            target.SendMessage("OnTouchUp", info, SendMessageOptions.DontRequireReceiver);
        }
        var previous = dict.GetValueOrDefault(info.fingerId);
        if (previous != null && previous.obj != null)
        {
            previous.obj.SendMessage("OnTouchDragEnd", previous.info, SendMessageOptions.DontRequireReceiver);
        }
        dict.Remove(info.fingerId);
    }

    void TouchMoved(TouchInfo info)
    {
        TouchState state = new();
        if (dict.TryGetValue(info.fingerId, out state))
        {
            state.info = info;
        }
    }

    private bool isPressed = true;
    private Vector2 lastTouchPosition = new(0f,0f);

    void Update()
    {
        // On mouse click
        if (!Input.mousePosition.Equals(lastTouchPosition))
        {
            lastTouchPosition = Input.mousePosition;
            TouchMoved(new TouchInfo { position = Input.mousePosition, fingerId = 275821 });
        }

        if (isPressed)
        {
            if (Input.GetMouseButtonUp(0))
            {
                TouchEnded(new TouchInfo { position = Input.mousePosition, fingerId = 275821 });
                isPressed = false;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                TouchBegan(new TouchInfo { position = Input.mousePosition, fingerId = 275821 });
                isPressed = true;
            }
        }

        // On touch
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            var info = new TouchInfo { position = touch.position, fingerId = touch.fingerId };
            if (touch.phase == TouchPhase.Began)
            {
                TouchBegan(info);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                TouchEnded(info);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                TouchMoved(info);
            }
        }

        // On drag
        foreach (var kvp in dict)
        {
            kvp.Value.obj.SendMessage("OnTouchDrag", kvp.Value.info, SendMessageOptions.DontRequireReceiver);
        }
    }
}
