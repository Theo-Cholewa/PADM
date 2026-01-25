using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class TeamServer : MonoBehaviour
{
    public TeamEnum TeamId;
    public TeamEnum EnnemyTeamId;
    public TeamGUI TeamGUI;
    public InventoryServer Inventory;

    public PartyTools.ValueServer<RessourceData> server;

    void Awake()
    {
        foreach(var iconTarget in TeamGUI.IconTargets) iconTarget.Value.Name = $"{iconTarget.Key}_{Team.Of(TeamId).id}";
    }

    void Start()
    {
        server = new (
            Party.current,
            $"team_{Team.Of(TeamId).id}",
            new RessourceData
            {
                gold = 0,
                wood = 0,
                rock = 0,
                chicken = 0,
                cannonLevel = 0,
                pirateLevel = 0,
                barrelLevel = 0,
                shipLevel = 0,
                health = 100,
            },
            (sharedData) => JsonUtility.ToJson(sharedData)
        );

        Party.current.OnMessage.AddListener(OnMessage);
    }

    void OnDestroy()
    {
        server.Dispose();
        Party.current?.OnMessage?.RemoveListener(OnMessage);
    }

    void OnMessage(PartyMessage message)
    {
        if (message.message.StartsWith("store;add;"))
        {
            var param = message.message.Split(';');
            if(param[2]!=Team.Of(TeamId).id)return;
            var value = int.Parse(param[3]);
            var typeName = param[4];
            var type = Enum.Parse<RessourceType>(typeName);
            SetResource(type, server.GetValue().Get(type) + value);
        }
    }

    public void SetResource(RessourceType type, int value)
    {
        var newValue = server.GetValue();
        newValue.Set(type, value);

        // Update Health
        if(type == RessourceType.Health)
        {
            newValue.health = value;
            TeamGUI.SetHealth(value);
        }

        // Update Money
        if(type == RessourceType.Gold)
        {
            newValue.gold = value;
            TeamGUI.SetGold(value);
        }

        // Update GUI
        var res = TeamGUI.GetRessourceCounter(type);
        if(res != null) res.SetCount(newValue.Get(type));

        var up = TeamGUI.GetUpgradeCounter(type);
        if(up != null) up.SetLevel(newValue.Get(type));
        
        
        server.SetValue(newValue);
    }

    async Task Kill()
    {
        await Inventory.server.SetValue(new GameStats{
            IsInFight = false,
            Winner = EnnemyTeamId,
            HasWinner = true,
        });
        Victory.Winner = Team.Of(EnnemyTeamId);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Victory");
    } 

    void Update()
    {
        var health = server.GetValue().health;
        if (health <= 0 && !Inventory.server.GetValue().HasWinner)
        {
            Kill();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            SetResource(RessourceType.Wood, server.GetValue().wood + 10);
            SetResource(RessourceType.Stone, server.GetValue().rock + 5);
            SetResource(RessourceType.Chicken, server.GetValue().chicken + 3);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SetResource(RessourceType.Gold, server.GetValue().gold + 10);
        }
    }
}
