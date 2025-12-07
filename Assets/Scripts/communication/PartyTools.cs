



using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

static class PartyTools
{
    
    public static Party GetParty(Scene scene)
    {
        foreach(var obj in scene.GetRootGameObjects())
        {
            var component = obj.GetComponentInChildren<Party>();
            if(component != null) return component;
        }
        return null;
    }


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

        void TellRole(PartyPeer peer)
        {
            party.SendMessage(peer, $"declare;add;{role}");
        }

        void TellNoRole(PartyPeer peer)
        {
            party.SendMessage(peer, $"declare;remove;{role}");
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

}