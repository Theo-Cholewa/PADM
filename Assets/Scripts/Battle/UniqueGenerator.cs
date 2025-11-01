using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UniqueGenerator : MonoBehaviour
{
    public GameObject PrefabToSpawn;
    public int timeToSpawn = 50 * 5;

    private int currentTime = 0;

    private GameObject currentSpawned;

    void FixedUpdate()
    {
        if (currentSpawned != null)
        {
            currentTime = 0;
            if (currentSpawned.IsDestroyed()) currentSpawned = null;
        }
        else
        {
            currentTime++;
            if (currentTime >= timeToSpawn)
            {
                currentSpawned = Instantiate(PrefabToSpawn);
                currentSpawned.transform.parent = transform;
                currentSpawned.transform.localPosition = new Vector3(0, 0, -0.01f);
                currentSpawned.transform.parent = null;
                currentSpawned.transform.localScale = new Vector3(3, 3, 3);
                currentTime = 0;
            }
        }
    }
}
