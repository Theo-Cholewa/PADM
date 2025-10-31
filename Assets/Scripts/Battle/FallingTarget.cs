using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class FallingTarget : MonoBehaviour
{

    public GameObject FallingResult;

    public class Effect: MonoBehaviour
    {

        public int time = 20;


        void FixedUpdate()
        {
            time--;
            if(time < 0)
            {
                Destroy(this);
            }
        }
    }

    public class State: Component
    {
        public int time;
    }

    private Collider collider;

    void Start()
    {
        collider = GetComponent<Collider>();
    }

    void OnCollisionStay(Collision collision)
    {
        var physic = collision.gameObject.GetComponent<Physic>();
        if (physic == null) return;

        if (physic.velocity.magnitude < 0.2f && collider.bounds.Contains(physic.transform.position))
        {
            var sub = physic.GetOrAddComponent<Effect>();
            sub.time += 2;

            var falling = physic.GetComponent<Falling>();
            if (falling != null)
            {
                falling.height = 1f;
            }

            if (sub.time > 60)
            {
                var result = Instantiate(FallingResult, physic.transform);
                result.transform.localScale = new(2, 2, 2);
                result.transform.parent = null;
                result.transform.rotation = new();
                Destroy(physic.gameObject);
            }
        }

    }

}
