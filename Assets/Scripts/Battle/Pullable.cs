using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Représente un objet physique attrapable et draggable
/// </summary>
[RequireComponent(typeof(Physic), typeof(Grabbable))]
public class Pullable : MonoBehaviour
{

    public Action onTake = null;

    private Physic physic;

    private Vector2 originalOffset;

    // Start is called before the first frame update
    void Start()
    {
        physic = GetComponent<Physic>();
    }

    void OnGrabStart(Grabbable pullable)
    {
        // On take callback
        if (pullable.grabHandList.Count == 1)
        {
            onTake?.Invoke();
        }
        // Handle rotation
        if (pullable.grabHandList.Count == 2)
        {
            var fingers = pullable.GetOrderedGrabHands().ToList();
            originalOffset = fingers[0].position - fingers[1].position;
        }
    }
    
    void OnGrabUpdate(Grabbable pullable)
    {
        var grabbings = pullable.GetOrderedGrabHands().ToList();

        // Handle translation
        foreach (var target in grabbings)
        {
            var direction = (target.position - transform.position).normalized/physic.weight;
            direction.z = 0;
            physic.velocity += direction * .1f;
        }
        
        // Handle rotation
        if (grabbings.Count >= 2)
        {
            var newOffset = grabbings[0].position - grabbings[1].position;
            transform.localRotation *= Quaternion.FromToRotation(originalOffset, newOffset);
            originalOffset = newOffset;
        }
    }
}
