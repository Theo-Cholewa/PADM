using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanonBall : MonoBehaviour
{

    Transform transform;

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
    }
    
    private int age = 0;
    
    void FixedUpdate()
    {
        var direction = transform.forward;
        transform.position += direction;

        age++;
        if(age>100) Destroy(transform.parent.gameObject);
    }
}
