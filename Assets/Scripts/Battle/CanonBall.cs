using UnityEngine;

public class CanonBall : MonoBehaviour
{

    private int age = 0;
    
    void FixedUpdate()
    {
        var direction = transform.forward;
        transform.position += direction * 2;

        age++;
        if(age>100) Destroy(transform.parent.gameObject);
    }
}
