using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pirate : MonoBehaviour
{
    public MeshRenderer Highlight;
    public MeshRenderer Colored;
    public Flag flag;
    public float MoveSpeed = 0.03f;

    void Start()
    {
        flag.pirates.Add(this);
        Colored.material.color = flag.color;
        Highlight.enabled = false;
    }

    public void Move(Vector3 direction)
    {
        var n = direction;
        n.z = 0;
        n.Normalize();
        transform.localRotation = Quaternion.FromToRotation(Vector3.down, n);
        var physic = GetComponent<Physic>();
        physic.velocity += n * MoveSpeed;
    }

    public void MoveTo(Vector3 position)
    {
        var direction = (position - transform.position);
        Move(direction);
    }


    public void SetHighlight(bool highlight)
    {
        Highlight.enabled = highlight;
    }
}
