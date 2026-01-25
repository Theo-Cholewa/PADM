using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class RessourceClient : MonoBehaviour
{
    public IconSender Sender;

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
        GameStats = new(Party.current,"game_stats", v => JsonUtility.FromJson<GameStats>(v));
    }

    void OnDestroy()
    {
        if(Teams!=null) foreach(var manager in Teams.Values) manager.Dispose();
        if(GameStats!=null) GameStats.Dispose();
        if(current == this) current = null;
    }


    /* TEAMS */
    private Dictionary<Team, TeamClient> Teams;

    public TeamClient Get(Team team)
    {
        if(Teams.TryGetValue(team, out var manager)) return manager;
        else
        {
            var man = new TeamClient(team,this);
            Teams[team] = man;
            return man;
        }
    }

    public class TeamClient
    {
        public Team team;
        public PartyTools.ValueClient<RessourceData> client;
        public RessourceClient root;

        public readonly UnityEvent onChange = new UnityEvent();

        public RessourceData? value
        {
            get{
                var values = client.GetValues();
                return values.Count==0 ? null : values.First().Value;
            }
        }


        public TeamClient(Team team, RessourceClient root)
        {
            this.team = team;
            this.root = root;

            client = new (
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

        public void SendIcon(Vector3 From, RessourceType type)
        {
            root.Sender.SpawnIcon(type.ToString(), root.Sender.SceneToWorld(From), $"{type}_{team.id}");
        }

        public Task AskForFight()
        {
            return Party.current.SendMessageToAll($"ask_fight;{team.id}");
        }

        public Task AskOpenShop()
        {
            return Party.current.SendMessageToAll($"ask_shop;{team.id}");
        }

        public Task AskCloseShop()
        {
            return Party.current.SendMessageToAll($"ask_shop_end;{team.id}");
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }

    /* PHASE */
    public PartyTools.ValueClient<GameStats> GameStats;

    public Task AskForFightEnd()
    {
        return Party.current.SendMessageToAll("ask_fight_end");
    }

    /// <summary>
    /// Change de scène selon l'état de la partie.
    /// </summary>
    public void GoToGoodScene()
    {
        if(GameStats.GetValues().Count == 0) return;
        
        var stats = GameStats.FirstOrDefault();
        if (stats.IsInFight)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Battle")
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Battle");
            }
        }
        else
        {
            if (!stats.HasWinner)
            {
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "RessourceTime")
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("RessourceTime");
                }
            }
            else
            {
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Sea")
                {
                    Sea.Team = stats.HasWinner ? Team.Of(stats.Winner) : null;
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Sea");
                }
            }
        }
    }
}
