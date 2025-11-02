using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Physic))]
public class GravitatedToward : MonoBehaviour
{
    public Transform target;
    public float strength = 1f;

    void FixedUpdate()
    {
        var physic = GetComponent<Physic>();
        var offset = (target.position - transform.position);
        offset.z = 0;
        physic.velocity += offset.normalized * strength;
    }
}
