using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BailOut : MonoBehaviour
{
    public GameObject waterBucket1;
    public GameObject waterBucket2;

    public GameObject bucket1; 
    public GameObject bucket2; 

    public GameObject puddle1;
    public GameObject puddle2;

    void Start()
    {
        waterBucket1.SetActive(false);
        waterBucket2.SetActive(false);
    }

    void Update()
    {
        if (puddle1.activeInHierarchy)
        {
            if (bucket1.activeInHierarchy && !waterBucket1.activeInHierarchy)
            {

                float distance = Vector3.Distance(bucket1.transform.position, puddle1.transform.position);
                float distanceMax = 0.5f;

                if (distance < distanceMax)
                {
                    puddle1.SetActive(false);
                    waterBucket1.SetActive(true);

                    waterBucket1.transform.position = bucket1.transform.position;
                    waterBucket1.transform.rotation = bucket1.transform.rotation;
                }
            }
        }

        if (puddle2.activeInHierarchy)
        {
            if (bucket2.activeInHierarchy && !waterBucket2.activeInHierarchy)
            {

                float distance = Vector3.Distance(bucket2.transform.position, puddle2.transform.position);
                float distanceMax = 0.5f;

                if (distance < distanceMax)
                {
                    puddle2.SetActive(false);
                    waterBucket2.SetActive(true);

                    waterBucket2.transform.position = bucket2.transform.position;
                    waterBucket2.transform.rotation = bucket2.transform.rotation;
                }
            }
        }
    }
}