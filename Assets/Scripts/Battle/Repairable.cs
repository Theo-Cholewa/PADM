using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repairable : MonoBehaviour
{

    public string Reparator;

    private bool repaired=false;


    void Update()
    {
        if (repaired)
        {
            transform.localScale = Vector3.Scale(transform.localScale, new(.95f, .95f, .95f));
            if (transform.localScale.x < 0.1)
            {
                Destroy(gameObject);
            }
        }
    }

    // Update is called once per frame
    void OnCollisionEnter(Collision collision)
    {
        if(!repaired && (collision.gameObject.GetComponent<Ammunition>()?.Tag.Contains(Reparator)??true))
        {
            repaired = true;
            Destroy(collision.gameObject);
        }
    }
}
