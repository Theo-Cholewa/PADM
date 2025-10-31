using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Représente un objet physique qui peut être poussé quand il entre en colision avec des Solid.
/// Il a une vélocité.
/// </summary>
[RequireComponent(typeof(Solid))]
public class Physic : MonoBehaviour
{

    public GameObject SpeedEffect = null;

    public float friction = 0.95f;

    public float weight = 1f;

    public Vector3 velocity = Vector3.zero;

    public bool hasPhysic = true;

    private GameObject wind_sphere = null;

    void Start()
    {
        if (SpeedEffect == null) SpeedEffect = Resources.Load<GameObject>("Prefab/wind_sphere");
    }

    void FixedUpdate()
    {
        if (!hasPhysic)
        {
            velocity.Set(0f, 0f, 0f);
            return;
        }

        // Movement
        transform.position += velocity;
        if(velocity.magnitude>1.5f) velocity *= 1f-(1-friction)/4f;
        else velocity *= friction;
        if (velocity.magnitude < 0.01) velocity = Vector3.zero;

        // Wind sphere
        if (SpeedEffect != null)
        {
            if (velocity.magnitude > 1.5f)
            {
                if (wind_sphere == null)
                {
                    wind_sphere = Instantiate(SpeedEffect, transform);
                    wind_sphere.transform.localScale = new(1.5f, 1.5f, 1.5f);
                }
                wind_sphere.transform.localPosition = -velocity + new Vector3(0f, 0f, -1f);
            }
            else
            {
                if (wind_sphere != null)
                {
                    Destroy(wind_sphere);
                    wind_sphere = null;
                }
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        var solid = collision.gameObject.GetComponent<Solid>();
        if (solid != null)
        {
            var strength = -Math.Min(0f, collision.GetContact(0).separation);
            var normal = collision.GetContact(0).normal / 10f * strength;
            
            normal.z = 0f;
            velocity += normal/weight;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.TryGetComponent<AudioSource>(out var audioSource)) audioSource.Play();
        
    }
}
