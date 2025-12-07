using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkDebugger : MonoBehaviour
{

    public TextMeshProUGUI IPAddressUI;
    public TextMeshProUGUI IdentifierUI;
    public TextMeshProUGUI PeerListUI;
    public TextMeshProUGUI MessageUI;
    public TMP_InputField InputUI;
    public Party party;

    [Serializable]
    public class ToggleZone
    {
        public String Role;
        public Toggle Toggle;
        public TextMeshProUGUI ToggleLabel;
        PartyTools.RoleServer RoleServer;
        PartyTools.RoleClient RoleClient;

        public void Init()
        {
            var party = PartyTools.GetParty(Toggle.gameObject.scene);

            RoleClient = new(party, s=>s==Role, OnChange, OnChange);

            Toggle.isOn = false;
            Toggle.onValueChanged.AddListener(isOn => {
                if(isOn)
                {
                    RoleServer = new(party,Role);
                }
                else
                {
                    RoleServer.Remove();
                    RoleServer = null;
                }
            });
        }

        public void OnChange(PartyPeer peer)
        {
            ToggleLabel.text = RoleClient.peers .Select(p=>p.name) .Aggregate("",(a,b)=>a+"\n"+b);
        }
    }

    public ToggleZone Screen;
    public ToggleZone Ship;

    // Start is called before the first frame update
    void Start()
    {
        var party = PartyTools.GetParty(gameObject.scene);
        Debug.Log($"Party initialized {party}");
        IPAddressUI.text = party.GetIPAddress().ToString();
        IdentifierUI.text = party.GetIdentifier();
        party.OnConnect.AddListener(OnPeerListChange);
        party.OnDisconnect.AddListener(OnPeerListChange);
        party.OnMessage.AddListener(OnMessage);
        InputUI.onSubmit.AddListener(OnInputSubmit);

        Screen.Init();
        Ship.Init();
    }

    void OnPeerListChange(PartyPeer _)
    {
        Debug.Log($"relist {party.GetPeers().Count}");
        var sb = new StringBuilder();
        foreach(var peer in party.GetPeers())
        {
            sb.AppendLine($"{peer.name}");
        }   
        PeerListUI.text = sb.ToString();
        PeerListUI.ForceMeshUpdate(true);
    }

    List<string> messages = new();

    void OnMessage(PartyMessage message)
    {
        Debug.Log(message.message);
        messages.Add($"[{message.peer.name}] : '{message.message}'");
        if(messages.Count>10)
        {
            messages.RemoveAt(0);
        }
        MessageUI.text = string.Join("\n", messages);
        MessageUI.ForceMeshUpdate(true);
    }

    void OnInputSubmit(string input)
    {
        party.SendMessageToAll(input);
        InputUI.text = "";
    }


}
