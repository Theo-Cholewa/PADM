using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repairable : MonoBehaviour
{

    public string Reparator;

    public GameObject RepairedPrefab;

    private bool repaired=false;


    void Update()
    {
        if (repaired)
        {
            if (RepairedPrefab != null)
            {
                var repaired = Instantiate(RepairedPrefab, transform);
                repaired.transform.localPosition = Vector3.zero;
                repaired.transform.localScale = Vector3.one;
                repaired.transform.localRotation = Quaternion.identity;
                repaired.transform.parent = null;
            }
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void OnCollisionEnter(Collision collision)
    {
        if(!repaired && (collision.gameObject.GetComponent<Ammunition>()?.Tag.Contains(Reparator)??false))
        {
            repaired = true;
            Destroy(collision.gameObject);
        }
    }
}
