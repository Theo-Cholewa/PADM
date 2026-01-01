using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


public class RessourceClient : MonoBehaviour
{
    /* LIFETIME */
    public static RessourceClient current;

    void Start()
    {
        if (current != null)
        {
            Destroy(this);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
        current = this;

        Teams = new Dictionary<Team, TeamClient>();
        FightStats = new(Party.current,"fight_stats", v => JsonUtility.FromJson<GameStats>(v));
    }

    void OnDestroy()
    {
        if(Teams!=null) foreach(var manager in Teams.Values) manager.Dispose();
        if(FightStats!=null) FightStats.Dispose();
    }


    /* TEAMS */
    private Dictionary<Team, TeamClient> Teams;

    public TeamClient Get(Team team)
    {
        if(Teams.TryGetValue(team, out var manager)) return manager;
        else
        {
            var man = new TeamClient(team);
            Teams[team] = man;
            return man;
        }
    }

    public class TeamClient
    {
        public Team team;
        public PartyTools.ValueClient<RessourceData> client;

        public readonly UnityEvent onChange = new UnityEvent();

        public RessourceData value
        {
            get => client.GetValues().FirstOrDefault().Value;
        }

        public TeamClient(Team team)
        {
            this.team = team;

            client = new PartyTools.ValueClient<RessourceData>(
                Party.current,
                $"team_{team.id}",
                v => JsonUtility.FromJson<RessourceData>(v)
            );

            client.onChange = (a,b)=>onChange.Invoke();
        }

        public Task Add(RessourceType type, int amount)
        {
            return Party.current.SendMessageToAll($"store;add;{team.id};{amount};{type}");
        }

        public Task AskForFight()
        {
            return Party.current.SendMessageToAll($"ask_fight;{team.id}");
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }

    /* PHASE */
    public PartyTools.ValueClient<GameStats> FightStats;

    public Task AskForFightEnd()
    {
        return Party.current.SendMessageToAll("ask_fight_end");
    }
}
