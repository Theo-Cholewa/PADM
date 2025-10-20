using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrewManager : MonoBehaviour
{
    [Header("Crew controllers a surveiller")]
    public CrewController[] crewControllers;

    private bool allActivated = false;

    void Update()
    {
        if (crewControllers == null || crewControllers.Length == 0)
            return;

        bool everyoneActive = true;

        foreach (var crew in crewControllers)
        {
            if (crew == null || !IsCrewActivated(crew))
            {
                everyoneActive = false;
                break;
            }
        }

        if (everyoneActive && !allActivated)
        {
            allActivated = true;
            Debug.Log("🚀 Toute l'équipe est activée !");
        }

        if (!everyoneActive && allActivated)
        {
            allActivated = false;
        }
    }

    private bool IsCrewActivated(CrewController crew)
    {
        var field = typeof(CrewController).GetField("allActivated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (bool)field.GetValue(crew);
    }
}
