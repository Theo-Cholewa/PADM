using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Canon : MonoBehaviour
{
    public GameObject projectile;
    private Collider collider;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Canon ready");
        collider = GetComponent<BoxCollider>();
    }

    void OnMouseDown()
    {
        var ptransform = transform.parent;

        Debug.Log("Clicked on the canon");
        var spawn = ptransform.position + ptransform.up * 5 * ptransform.lossyScale.y;
        
        var instance = Instantiate(projectile, spawn, ptransform.rotation);
        instance.transform.localScale = ptransform.localScale/5;


    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
