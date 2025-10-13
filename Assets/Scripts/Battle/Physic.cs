using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Représente un objet physique qui peut être poussé quand il entre en colision avec des Solid.
/// Il a une vélocité.
/// </summary>
[RequireComponent(typeof(Solid))]
public class Physic : MonoBehaviour
{

    public float friction = 0.95f;

    public Vector3 velocity = Vector3.zero;

    void FixedUpdate()
    {
        transform.position += velocity;
        velocity *= friction;
        if (velocity.magnitude < 0.01) velocity = Vector3.zero;
    }

    void OnCollisionStay(Collision collision)
    {
        var solid = collision.gameObject.GetComponent<Solid>();
        if (solid != null)
        {
            velocity += collision.GetContact(0).normal/10f;
        }
    }
}
