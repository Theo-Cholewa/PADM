using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CenterState
{
    MARKET,
    IN_BATTLE,
    IN_TRAVEL,
}

public class Center : MonoBehaviour
{
    public CenterState DefaultState = CenterState.IN_TRAVEL;
    public GameObject Market;
    public GameObject InBattle;
    public GameObject InTravel;
    public Hider Hider;

    public async void SetState(CenterState state)
    {
        Debug.Log("Avant " + state);
        await Hider.HideAndShow();
        Debug.Log("Après " + state);
        Market.SetActive(state == CenterState.MARKET);
        InBattle.SetActive(state == CenterState.IN_BATTLE);
        InTravel.SetActive(state == CenterState.IN_TRAVEL);
    }

    void Start()
    {
        Market.SetActive(DefaultState == CenterState.MARKET);
        InBattle.SetActive(DefaultState == CenterState.IN_BATTLE);
        InTravel.SetActive(DefaultState == CenterState.IN_TRAVEL);
    }
}
