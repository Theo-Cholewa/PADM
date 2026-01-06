using UnityEngine;

public class Inventory : MonoBehaviour
{
    public TeamInventory Red;
    public TeamInventory Blue;
    public Center Center;

    private TeamInventory TeamOnShop;

    public TeamInventory GetInventory(Team team)
    {
        if(team == Team.RED) return Red;
        else return Blue;
    }

    public void SetShopper(Team team)
    {
        var NewTeamOnShop = GetInventory(team);
        if (TeamOnShop != NewTeamOnShop)
        {
            if(TeamOnShop!=null) TeamOnShop.SetUpgradable(false);
            if (NewTeamOnShop != null)
            {
                NewTeamOnShop.SetUpgradable(true);
                Center.SetState(CenterState.MARKET);
            }
            else
            {
                Center.SetState(CenterState.IN_TRAVEL);
            }
        }
        TeamOnShop = NewTeamOnShop;
    }

    public void SetInBattle()
    {
        if(TeamOnShop!=null) TeamOnShop.SetUpgradable(false);
        Center.SetState(CenterState.IN_BATTLE);
        TeamOnShop = null;
    }


}
