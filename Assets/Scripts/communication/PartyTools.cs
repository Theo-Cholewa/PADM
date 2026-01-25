using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public static class PartyTools
{

    /// <summary>
    /// Indique un rôle donné aux autres membres de la Party.
    /// </summary>
    public class RoleServer
    {

        Party party;
        string role;

        public RoleServer(Party party, string role)
        {
            this.party = party;
            this.role = role;
            
            foreach(var peer in party.GetPeers()) TellRole(peer);
            party.OnConnect.AddListener(OnConnect);
        }

        public void Remove()
        {
            foreach(var peer in party.GetPeers()) TellNoRole(peer);
            party.OnConnect.RemoveListener(OnConnect);
        }

        void OnConnect(PartyPeer peer)
        {
            TellRole(peer);
        }

        Task TellRole(PartyPeer peer)
        {
            return party.SendMessage(peer, $"declare;add;{role}");
        }

        Task TellNoRole(PartyPeer peer)
        {
            return party.SendMessage(peer, $"declare;remove;{role}");
        }

    }

    /// <summary>
    /// Liste les autres membres de la Party qui ont un rôle accepté par un prédicat.
    /// </summary>
    public class RoleClient
    {
        Party party;
        Predicate<string> role;
        Action<PartyPeer> onAdd;
        Action<PartyPeer> onRemove;

        public List<PartyPeer> peers = new();

        public RoleClient(
            Party party,
            Predicate<string> role,
            Action<PartyPeer> onAdd,
            Action<PartyPeer> onRemove
        )
        {
            this.party = party;
            this.role = role;
            this.onAdd = onAdd;
            this.onRemove = onRemove;
            party.OnMessage.AddListener(OnReceive);
        }

        public void Remove()
        {
            party.OnMessage.RemoveListener(OnReceive);
        }

        void OnReceive(PartyMessage message)
        {
            if (!message.message.StartsWith("declare;"))return;
            var msg = message.message.Substring("declare;".Length);

            var param = msg.Split(";");
            if(param.Length!=2)return;

            if (role(param[1]))
            {
                if (param[0] == "add")
                {
                    peers.Add(message.peer);
                    message.peer.OnDisconnect.AddListener(OnDisconnect);
                    onAdd(message.peer);
                }
                else if (param[0] == "remove")
                {
                    peers.Remove(message.peer);
                    message.peer.OnDisconnect.RemoveListener(OnDisconnect);
                    onRemove(message.peer);
                }
            }
        }

        void OnDisconnect(PartyPeer peer)
        {
            peers.Remove(peer);
            onRemove(peer);
        }
    }

    /// <summary>
    /// Permet d'accèder aux valeurs d'une variable partagée.
    /// </summary>
    /// <typeparam name="T">Le type de la variable partagée.</typeparam>
    public class ValueClient<T>
    {
        private Dictionary<PartyPeer,T> values = new();
        private Party party;
        private string name;
        private Func<string,T> fromStr;
        public Action<PartyPeer,T> onSet = null;
        public Action<PartyPeer,T> onAdd = null;
        public Action<PartyPeer,T> onRemove = null;
        public Action<PartyPeer,T> onChange = null;
        
        public ValueClient(Party party, string name, Func<string,T> fromStr)
        {
            this.party = party;
            this.fromStr = fromStr;
            this.name = name;
            party.OnMessage.AddListener(OnMessage);
            party.OnDisconnect.AddListener(OnDisconnect);
            party.OnConnect.AddListener(OnConnect);
            party.SendMessageToAll("value;ask");
        }

        public void Dispose()
        {
            party.OnMessage.RemoveListener(OnMessage);
            party.OnDisconnect.RemoveListener(OnDisconnect);
            party.OnConnect.RemoveListener(OnConnect);
        }
        
        void OnMessage(PartyMessage message)
        {
            if (message.message.StartsWith($"value;{name};"))
            {
                var param = message.message.Substring($"value;{name};".Length).Split(";");
                if(param.Length!=2)return;
                var opt = param[0];

                if (opt == "set")
                {
                    var value = fromStr(param[1]);
                    var added = !values.ContainsKey(message.peer);
                    values[message.peer] = value;
                    if(added){
                        if(onAdd!=null) onAdd(message.peer, value);
                    }
                    else
                    {
                        if(onSet!=null) onSet(message.peer, value);
                    }
                    if(onChange!=null) onChange(message.peer, value);
                }
                else if (opt == "remove")
                {
                    if(values.TryGetValue(message.peer, out var value))
                    {
                        values.Remove(message.peer);
                        if(onRemove!=null) onRemove(message.peer, value);
                        if(onChange!=null) onChange(message.peer, value);
                    }
                    
                }
            }
        }

        void OnConnect(PartyPeer peer)
        {
            party.SendMessage(peer,"value;ask");
        }

        void OnDisconnect(PartyPeer peer)
        {
            if(values.TryGetValue(peer, out var value))
            {
                values.Remove(peer);
                if(onRemove!=null) onRemove(peer, value);
                if(onChange!=null) onChange(peer, value);
            }
        }

        public Dictionary<PartyPeer,T> GetValues()
        {
            return values;
        }

        public T GetAggregate(Func<T,T,int,T> aggregator, T defaultValue)
        {
            return values.Values.Count==0 ? defaultValue : values.Values.Aggregate((a,b)=>aggregator(a,b,values.Values.Count));
        }

        public T FirstOrDefault()
        {
            return values.Values.Count == 0 ? default : values.Values.First();
        }
    }

    /// <summary>
    /// Permet de modifier la valeur d'une variable partagée.
    /// </summary>
    /// <typeparam name="T">Le type de la variable partagée.</typeparam>
    public class ValueServer<T>
    {
        Party party;
        Func<T,string> toStr;
        string name;
        T value;

        public ValueServer(Party party, string name, T defaultValue, Func<T,string> toStr)
        {
            this.party = party;
            this.name = name;
            this.toStr = toStr;
            value = defaultValue;
            this.party.SendMessageToAll($"value;{name};set;{toStr(value)}");
            this.party.OnMessage.AddListener(OnMessage);
        }

        public void Dispose()
        {
            party.OnMessage.RemoveListener(OnMessage);
        }

        public Task SetValue(T value)
        {
            this.value = value;
            return party.SendMessageToAll($"value;{name};set;{toStr(value)}");
        }

        public T GetValue()
        {
            return this.value;
        }

        public Task Remove()
        {
            return party.SendMessageToAll($"value;{name};remove;no");
        }

        public void OnMessage(PartyMessage msg)
        {
            if (msg.message == "value;ask")
            {
                party.SendMessage(msg.peer, $"value;{name};set;{toStr(value)}");
            }
        }
    }
    
    /// <summary>
    /// Permet de définir un "role" qu'un seule Peer peut avoir dans la Party.
    /// Le callback onAcquired est appelé si le rôle est ascquis.
    /// 
    /// Normalement il y a peu de chance que le role soit ensuite retiré.
    /// Mais si c'est le cas, le callback onWithdraw est appelé.
    /// 
    /// Le callback onRefused est appelé si le rôle est refusé initialement. Il est possible
    /// que le rôle soit finalement donné après coups.
    /// 
    /// </summary>
    public class UniqueRole{

        Party party;

        string role;

        ValueServer<long> server;

        ValueClient<long> client;

        Action onAcquired;

        Action onRefused;

        Action onWithdraw;

        long id;

        bool canAcquire = false;
        bool firstTest = true;

        bool haveTheRole = false;

        bool isDisposed = false;

        public UniqueRole(Party party, MonoBehaviour behav, string role, Action onAcquired, Action onRefused, Action onWithdraw)
        {
            this.party = party;
            this.role = role;
            this.onAcquired = onAcquired;
            this.onWithdraw = onWithdraw;
            this.onRefused = onRefused;

            id = DateTime.UtcNow.Ticks;

            client = new(party, $"unique_role_{role}", it=>long.Parse(it));
            client.onChange = onChange;
            
            server = new(party, $"unique_role_{role}", id, it=>it.ToString());

            behav.StartCoroutine(WaitForAcquire());
        }

        public void Dispose()
        {
            isDisposed = true;
            client.Dispose();
            server.Dispose();
        }

        public bool HaveTheRole => haveTheRole;

        private void onChange(PartyPeer rpeer, long value)
        {
            if(!canAcquire)return;

            Debug.Log(id+" : "+client.GetValues().Values.Select(it=>it.ToString()).Aggregate("",(a,b)=>a+","+b));

            // If someone already have this role, I cannot get the role
            if (client.GetValues().Values.Any(it => it < id))
            {
                if(haveTheRole) onWithdraw?.Invoke();
                haveTheRole = false;
            }

            // If no one already have this role, I take it
            else if (client.GetValues().Values.All(it => it > id))
            {
                if(!haveTheRole) onAcquired?.Invoke();
                haveTheRole = true;
            }

            // If someone have the same id, I get a new id
            else if (client.GetValues().Values.Any(it => it == id))
            {
                id = DateTime.UtcNow.Ticks+UnityEngine.Random.Range(0,5);
                server.SetValue(id);
            }

            if(firstTest)
            {
                Debug.Log($"FIRST TEST ${haveTheRole}");
                firstTest = false;
                if(!haveTheRole) onRefused?.Invoke();
            }
        }

        private IEnumerator WaitForAcquire()
        {
            yield return new WaitForSeconds(.5f);
            if (!isDisposed)
            {
                canAcquire = true;
                onChange(null, 0);
            }
            
        }
    }

}