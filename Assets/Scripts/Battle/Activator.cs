using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Grabbable))]
public class Activator : MonoBehaviour
{
    public List<GameObject> Targets;
    public AudioClip ActivationSound;
    public float ActivationDistance = 50f;
    public int MaxHandPower = 6;
    private int MyId = 0;
    private static int IdCounter = 0;



    public class ActivatorTarget: MonoBehaviour
    {
        public int Count=0;
        public HashSet<int> ActiveHands = new HashSet<int>();
        public HashSet<int> ActivateActivator = new HashSet<int>();
        public int Grabbing = 0;
    }

    private List<ActivatorTarget> TargetsData;


    void Start()
    {
        MyId = IdCounter++;
        Targets.RemoveAll(it=>it.IsDestroyed());
        TargetsData = Targets.Select(target => {
            var data = target.GetOrAddComponent<ActivatorTarget>();
            data.Count++;
            return data;
        }).ToList();
    }
    
    void OnGrabStart(Grabbable grabbable)
    {
        Clear();
        foreach(var data in TargetsData) data.Grabbing++;
    }

    void OnGrabEnd(Grabbable grabbable)
    {
        Clear();
        for(var i = 0; i < TargetsData.Count; i++)
        {
            var data = TargetsData[i];
            var target = Targets[i];
            data.Grabbing--;
            if (data.Grabbing == 0)
            {
                // Try to Shoot
                if (data.ActivateActivator.Count == data.Count)
                {
                    var count = data.ActiveHands.Count;
                    var strength = Math.Min(count * 1f / MaxHandPower, 1f);
                    Debug.Log($"Activate {target.name} with power of {strength}");
                    target.SendMessage("OnActivate", strength);
                }
                
                data.ActiveHands.Clear();
                data.ActivateActivator.Clear();
            }
        }
        
    }

    void OnGrabUpdate(Grabbable grabbable)
    {
        Clear();
        for(var i=0; i<TargetsData.Count; i++)
        {
            var data = TargetsData[i];
            foreach(var hand in grabbable.GetOrderedGrabHands())
            {
                var distance = Vector3.Distance(hand.position, transform.position);
                if (distance > ActivationDistance)
                {
                    if (data.ActiveHands.Add(hand.fingerId))
                    {
                        if (ActivationSound != null) GetComponent<AudioSource>().PlayOneShot(ActivationSound);
                    }
                    data.ActivateActivator.Add(MyId);
                }
            }
        }
    }

    void Clear()
    {
        while (true)
        {
            var index = Targets.FindIndex(it=>it.IsDestroyed());
            if(index==-1)break;
            Targets.RemoveAt(index);
            TargetsData.RemoveAt(index);
        }
    }

}
