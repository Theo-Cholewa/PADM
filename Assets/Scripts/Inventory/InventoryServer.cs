using UnityEngine;

public class InventoryServer : MonoBehaviour
{
    public Inventory InventoryGUI;
    public PartyTools.ValueServer<GameStats> server;
    public Team TeamOnShop = null;

    void SetTeamOnShop(Team team)
    {
        TeamOnShop = team;
        InventoryGUI.SetShopper(team);
    }

    void SetBattle()
    {
        TeamOnShop = null;
        InventoryGUI.SetInBattle();
    }

    void Start()
    {
        server = new(
            Party.current,
            "game_stats",
            new GameStats{
                IsInFight = false,
                Winner = null,
            },
            v => JsonUtility.ToJson(v)
        );
        Party.current.OnMessage.AddListener(OnMessage);
    }

    void OnDestroy()
    {
        server.Dispose();
        Party.current.OnMessage.RemoveListener(OnMessage);
    }

    void OnMessage(PartyMessage msg)
    {
         // Start fight
        if (msg.message.StartsWith("ask_fight;"))
        {
            if (!server.GetValue().IsInFight)
            {
                server.SetValue(new GameStats{
                    IsInFight = true,
                    Winner = null,
                });
                SetBattle();
            }
        }

        // End fight
        else if (msg.message.StartsWith("ask_fight_end"))
        {
            if (server.GetValue().IsInFight)
            {
                server.SetValue(new GameStats{
                    IsInFight = false,
                    Winner = null,
                });
                SetTeamOnShop(null);
            }
        }

        // On Open Shop
        else if (msg.message.StartsWith("ask_shop;"))
        {
            var team = Team.Parse(msg.message.Substring("ask_shop;".Length));
            if (TeamOnShop == null)
            {
                SetTeamOnShop(team);
            }
        }

        // On Close Shop
        else if (msg.message.StartsWith("ask_shop_end;"))
        {
            var team = Team.Parse(msg.message.Substring("ask_shop_end;".Length));
            if (TeamOnShop == team)
            {
                SetTeamOnShop(null);
            }
        }
    }

}