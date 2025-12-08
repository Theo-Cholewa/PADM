using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicationFlag : MonoBehaviour
{

    public MeshRenderer Colored;

    void OnChange(Team newTeam)
    {
        Colored.material.color = newTeam.color;
    }

    void OnEnable()
    {
        Colored.material.color = Team.currentTeam.color;
        Team.onTeamChanged.AddListener(OnChange);
    }

    void OnDisable()
    {
        Team.onTeamChanged.RemoveListener(OnChange);
    }
}