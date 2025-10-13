using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Un objet physique qui chute après avoir été immobile pendant trop longtemps.
/// Après être tombé, il se transforme en un autre objet.
/// </summary>
[RequireComponent(typeof(Physic))]
public class Falling : MonoBehaviour
{
    /// <summary>
    /// L'objet qui remplace cet objet après sa chute.
    /// </summary>
    public GameObject Fell;

    private Physic _physic;

    private float height = 1f;

    private void Start()
    {
        _physic = GetComponent<Physic>();
    }

    private void FixedUpdate()
    {
        if (_physic.velocity.magnitude < 0.2f)
        {
            height -= 0.01f;
            if (height < 0f)
            {
                var created = Instantiate(Fell, transform);
                created.transform.localScale = new(1.5f, 1.5f, 1.5f);
                created.transform.parent = null;
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            height += 0.05f;
            if (height > 1f) height = 1f;
        }
        gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, height*360f);
    }
}
