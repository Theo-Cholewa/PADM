using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Innondation : MonoBehaviour
{
    public GameObject puddle1;
    public GameObject puddle2;
    public GameObject hole1;
    public GameObject hole2;

    private float troue1Timer = 0f;
    private float troue2Timer = 0f;
    private const float waterDelay = 10f;

    void Start()
    {
        puddle1.SetActive(false);
        puddle2.SetActive(false);
    }

    void Update()
    {
        if (hole1.activeInHierarchy)
        {
            troue1Timer += Time.deltaTime;
            if (troue1Timer >= waterDelay)
            {
                puddle1.SetActive(true);
            }
        }
        else
        {
            troue1Timer = 0f;
        }

        if (hole2.activeInHierarchy)
        {
            troue2Timer += Time.deltaTime;
            if (troue2Timer >= waterDelay)
            {
                puddle2.SetActive(true);
            }
        }
        else
        {
            troue2Timer = 0f;
        }
    }
}