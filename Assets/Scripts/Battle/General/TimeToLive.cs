using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeToLive : MonoBehaviour
{
    
    public int timeToLive = 100;

    void FixedUpdate()
    {
        timeToLive--;
        if (timeToLive <= 0)
        {
            Destroy(gameObject);
        }
    }
}
