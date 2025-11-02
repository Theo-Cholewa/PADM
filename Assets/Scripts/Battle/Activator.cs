using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Grabbable))]
public class Activator : MonoBehaviour
{
    public GameObject Target;
    public AudioClip ActivationSound;
    public float ActivationDistance = 50f;
    public int MaxHandPower = 6;
    private int MyId = 0;



    public class ActivatorTarget: MonoBehaviour
    {
        public int Count=0;
        public HashSet<int> ActiveHands = new HashSet<int>();
        public HashSet<int> ActivateActivator = new HashSet<int>();
        public int Grabbing = 0;
    }

    private ActivatorTarget TargetData;


    void Start()
    {
        TargetData = Target.GetOrAddComponent<ActivatorTarget>();
        TargetData.Count++;
        MyId = TargetData.Count - 1;
    }
    
    void OnGrabStart(Grabbable grabbable)
    {
        TargetData.Grabbing++;
    }

    void OnGrabEnd(Grabbable grabbable)
    {
        TargetData.Grabbing--;
        if (TargetData.Grabbing == 0)
        {
            // Try to Shoot
            if (TargetData.ActivateActivator.Count == TargetData.Count)
            {
                var count = TargetData.ActiveHands.Count;
                var strength = Math.Min(count * 1f / MaxHandPower, 1f);
                Debug.Log($"Activate {Target.name} with power of {strength}");
                Target.SendMessage("OnActivate", strength);
            }
            
            TargetData.ActiveHands.Clear();
            TargetData.ActivateActivator.Clear();
        }
    }

    void OnGrabUpdate(Grabbable grabbable)
    {
        foreach(var hand in grabbable.GetOrderedGrabHands())
        {
            var distance = Vector3.Distance(hand.position, transform.position);
            if (distance > ActivationDistance)
            {
                if (TargetData.ActiveHands.Add(hand.fingerId))
                {
                    if (ActivationSound != null) GetComponent<AudioSource>().PlayOneShot(ActivationSound);
                }
                TargetData.ActivateActivator.Add(MyId);
            }
        }
    }

}
