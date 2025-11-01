using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Représente un objet physique attrapable et draggable
/// </summary>
[RequireComponent(typeof(Physic), typeof(Pullable))]
public class Grabbable : MonoBehaviour
{

    public Action onTake = null;

    private Physic physic;

    // Start is called before the first frame update
    void Start()
    {
        physic = GetComponent<Physic>();
    }

    void OnPullingStart(Pullable pullable)
    {
        if (pullable.fingers.Count == 1)
        {
            onTake?.Invoke();
        }
    }
    
    void OnPullingUpdate(Pullable pullable)
    {
        foreach(var target in pullable.GetPullings())
        {
            var direction = (target.position - transform.position).normalized;
            direction.z = 0;
            physic.velocity += direction * .1f;
        }
    }
}
