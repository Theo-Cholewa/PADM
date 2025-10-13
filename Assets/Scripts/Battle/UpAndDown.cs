using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpAndDown : MonoBehaviour
{
    public float strength;

    public float speed;

    private int time=0;

    // Update is called once per frame
    void Update()
    {
        var step = Mathf.Sin(time*speed/1000f)*strength;
        transform.position += new Vector3(0, step, 0);
        time++;
    }
}
