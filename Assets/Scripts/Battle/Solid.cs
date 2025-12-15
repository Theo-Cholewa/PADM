using UnityEngine;

/// <summary>
/// Représente un objet physic qui a des collisions avec d'autres objets physiques.
/// </summary>
[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Collider))]
public class Solid : MonoBehaviour
{

    public bool DoPush = true;

    // Start is called before the first frame update
    void Start()
    {
        var rigidBody = GetComponent<Rigidbody>();
        rigidBody.useGravity = false;
        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
    }
}
