using UnityEngine;

public class InventoryGUI : MonoBehaviour
{
    public TeamGUI Red;
    public TeamGUI Blue;
    public Center Center;

    private TeamGUI TeamOnShop;
    private bool IsBattle=false;

    public TeamGUI GetInventory(Team team)
    {
        if(team == Team.RED) return Red;
        else if(team == Team.BLUE) return Blue;
        else return null;
    }

    public void SetShopper(Team team)
    {
        var NewTeamOnShop = GetInventory(team);
        if (TeamOnShop != NewTeamOnShop || IsBattle)
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
        IsBattle = false;
    }

    public void SetInBattle()
    {
        if(TeamOnShop!=null) TeamOnShop.SetUpgradable(false);
        Center.SetState(CenterState.IN_BATTLE);
        TeamOnShop = null;
        IsBattle = true;
    }


}
