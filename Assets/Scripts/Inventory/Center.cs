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
        await Hider.HideAndShow();
        Market.transform.localScale = state == CenterState.MARKET ? Vector3.one : Vector3.zero;
        InBattle.transform.localScale = state == CenterState.IN_BATTLE ? Vector3.one : Vector3.zero;
        InTravel.transform.localScale = state == CenterState.IN_TRAVEL ? Vector3.one : Vector3.zero;
    }

    void Start()
    {
        Market.transform.localScale = DefaultState == CenterState.MARKET ? Vector3.one : Vector3.zero;
        InBattle.transform.localScale = DefaultState == CenterState.IN_BATTLE ? Vector3.one : Vector3.zero;
        InTravel.transform.localScale = DefaultState == CenterState.IN_TRAVEL ? Vector3.one : Vector3.zero;
    }
}
