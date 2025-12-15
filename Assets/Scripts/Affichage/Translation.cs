using UnityEngine;

public class Translation : MonoBehaviour
{
    public float Duration;
    public Vector3 Movement;

    private float CurrentTime=0f;

    void Update()
    {
        transform.position += Movement * Time.deltaTime;
        CurrentTime += Time.deltaTime;
        if(CurrentTime >= Duration)
        {
            Destroy(gameObject);
        }
    }
}
