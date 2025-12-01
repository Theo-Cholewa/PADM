using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEmitter : MonoBehaviour
{
    Sprite image;
    float size;
    float power;
    int count;

    void Start()
    {
        for(int i = 0; i < count; i++)
        {
            var particle = new GameObject("particle");

        }
        Destroy(gameObject);
    }
}
