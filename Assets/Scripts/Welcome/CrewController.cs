using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrewController : MonoBehaviour
{
    [Header("Child Components")]
    public HandController[] handControllers;
    public TouchIndicatorWave[] touchIndicators;

    private bool allActivated = false;

    void Update()
    {
        if ((handControllers == null || handControllers.Length == 0) &&
            (touchIndicators == null || touchIndicators.Length == 0))
            return;

        bool everyoneActive = true;

        foreach (var hand in handControllers)
        {
            if (hand == null || !hand.activated)
            {
                everyoneActive = false;
                break;
            }
        }

        if (everyoneActive)
        {
            foreach (var touch in touchIndicators)
            {
                if (touch == null || !touch.isTouched)
                {
                    everyoneActive = false;
                    break;
                }
            }
        }

        if (everyoneActive && !allActivated)
        {
            allActivated = true;
            Debug.Log("👥 Tous les membres de l'équipage sont activés !");
        }

        if (!everyoneActive && allActivated)
        {
            allActivated = false;
        }
    }
}
