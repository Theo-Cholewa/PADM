using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float speed = 10f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new(0f, 0f, speed));
    }
}
