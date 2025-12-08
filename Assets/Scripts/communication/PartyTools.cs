



using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

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

        public void Remove()
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

        public void Destroy()
        {
            this.party.OnMessage.RemoveListener(OnMessage);
        }

        public Task SetValue(T value)
        {
            this.value = value;
            return this.party.SendMessageToAll($"value;{name};set;{toStr(value)}");
        }

        public Task Remove()
        {
            return this.party.SendMessageToAll($"value;{name};remove;no");
        }

        public void OnMessage(PartyMessage msg)
        {
            if (msg.message == "value;ask")
            {
                party.SendMessage(msg.peer, $"value;{name};set;{toStr(value)}");
            }
        }
    }

}