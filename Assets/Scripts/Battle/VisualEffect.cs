using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualEffect : MonoBehaviour
{
    IEnumerator Animation()
    {
        var scale = transform.localScale;
        for (var i = 0.05f; i<=1f; i+=0.05f)
        {
            transform.localScale = scale * i;
            yield return null;
        }
        for (var i = 1f; i >= 0f; i -= 0.05f)
        {
            transform.localScale = scale * i;
            yield return null;
        }
        Destroy(gameObject);
        
    }

    void Start()
    {
        StartCoroutine(Animation());
    }
}
