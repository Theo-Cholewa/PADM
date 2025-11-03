using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class TouchInfo
{
    public Vector2 position;
    public int fingerId;
}

public class GlobalTouchInfo
{
    public TouchInfo info;
    public bool doCancel = false;
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

        if (Physics.Raycast(ray, out hit))
        {
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

    private Dictionary<int, TouchInfo> touched = new();


    bool SendGlobal(string message, TouchInfo info)
    {
        var state = new GlobalTouchInfo { info = info };
        foreach(var o in gameObject.scene.GetRootGameObjects())
        {
            o.BroadcastMessage(message, state, SendMessageOptions.DontRequireReceiver);
        }
        return !state.doCancel;
    }


    void TouchBegan(TouchInfo info)
    {
        touched[info.fingerId] = info;
        if (SendGlobal("OnGlobalTouchDown", info))
        {
            var target = GetTarget(info.position);
            if (target != null)
            {
                target.SendMessage("OnTouchDown", info, SendMessageOptions.DontRequireReceiver);
                dict[info.fingerId] = new TouchState { obj = target, info = info };
            }
        }
    }

    void TouchEnded(TouchInfo info)
    {
        touched.Remove(info.fingerId);
        if (SendGlobal("OnGlobalTouchUp", info))
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
    }

    void TouchMoved(TouchInfo info)
    {
        if (touched.TryGetValue(info.fingerId, out var touchedpt))
        {
            touchedpt.position = info.position;
        }
        
        TouchState state = new();
        if (dict.TryGetValue(info.fingerId, out state))
        {
            state.info = info;
        }
    }

    private bool isPressed = false;
    private bool isPressed2 = false;
    private bool isPressed3 = false;
    private int lastPressedMouseFingerId = 275821;
    private Vector2 lastTouchPosition = new(0f, 0f);

    void Start()
    {
        Input.simulateMouseWithTouches = false;
    }

    void Update()
    {
        // On mouse click
        if (Input.touchCount == 0)
        {
            if (!Input.mousePosition.Equals(lastTouchPosition))
            {
                lastTouchPosition = Input.mousePosition;
                TouchMoved(new TouchInfo { position = Input.mousePosition, fingerId = lastPressedMouseFingerId });
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
                    lastPressedMouseFingerId = 275821;
                }
            }

            if (isPressed2)
            {
                if (Input.GetMouseButtonUp(1))
                {
                    TouchEnded(new TouchInfo { position = Input.mousePosition, fingerId = 275822 });
                    isPressed2 = false;
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(1))
                {
                    TouchBegan(new TouchInfo { position = Input.mousePosition, fingerId = 275822 });
                    isPressed2 = true;
                    lastPressedMouseFingerId = 275822;
                }
            }

            if (isPressed3)
            {
                if (Input.GetMouseButtonUp(2))
                {
                    TouchEnded(new TouchInfo { position = Input.mousePosition, fingerId = 275823 });
                    isPressed3 = false;
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(2))
                {
                    TouchBegan(new TouchInfo { position = Input.mousePosition, fingerId = 275823 });
                    isPressed3 = true;
                    lastPressedMouseFingerId = 275823;
                }
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
            if (kvp.Value.obj.IsDestroyed())
            {
                dict.Remove(kvp.Key);
            }
            else kvp.Value.obj.SendMessage("OnTouchDrag", kvp.Value.info, SendMessageOptions.DontRequireReceiver);
        }
    }
}
