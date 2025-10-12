using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{

    public GameObject Created;

    private  GameObject content;

    void Start()
    {
        refill();
    }

    void refill()
    {
        content = Instantiate(Created);
        content.transform.parent = gameObject.transform;
        content.transform.localPosition = new(0, 0, 0);
        content.transform.localScale = new(.8f, .8f, .8f);
        content.transform.localRotation = new();
        content.transform.parent = null;

        content.GetComponent<Draggable>().onTake = () => {
            content.GetComponent<Draggable>().onTake = null;
            refill();
        };
    }

    void Update()
    {
        
    }
}
