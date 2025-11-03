using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispersionNuage : MonoBehaviour
{
    public int Duration;

    private Vector3 direction;
    private Vector3 start;

    // Start is called before the first frame update
    void Start()
    {
        start = transform.localPosition;
        direction = transform.localPosition;
        direction.y = 0;
        direction.Normalize();
        StartCoroutine(Animate());
    }
    
    IEnumerator Animate()
    {
        for (var now = 0; now < Duration; now++)
        {
            var advancement = now / (float)Duration;
            transform.localPosition = start + direction * advancement*10;
            yield return null;
        }
    }
}
