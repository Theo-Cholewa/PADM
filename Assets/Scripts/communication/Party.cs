using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;


public struct PartyMessage
{
    public PartyPeer peer;
    public string message;
}

public class PartyPeer
{
    public string name;
    public UnityEvent<PartyPeer> OnDisconnect = new();
    public UnityEvent<(string,string)> OnMessage = new();
}

class Client
{
    public TcpClient client;
    public NetworkStream stream;
    public byte[] buffer = new byte[512];
    public int end = 0;
    public int end_check = 0;
    public PartyPeer guest;
    public IPAddress addr;
}

public class Party : MonoBehaviour
{
    public static Party current;

    [Header("Identity")]
    public string PartyName = "Unknown";

    public int UdpPort = 20003;
    public int TcpPort = 20004;


    // PUBLIC API //
    [Header("Events")]
    public UnityEvent<PartyMessage> OnMessage = new();
    public UnityEvent<PartyPeer> OnConnect = new();
    public UnityEvent<PartyPeer> OnDisconnect = new();

    public string GetIdentifier()
    {
        return Identifier;
    }

    public IPAddress GetIPAddress()
    {
        return myself;
    }

    public async Task SendMessage(PartyPeer target, string message)
    {
        // Get clients
        var targets = clients.Where(c => c.guest == target).ToList();
        if(targets.Count==0) return;
        
        // Send
        var content = $"{message}\n";
        var tasks = targets.Select(target => SendPacket(target, content)).ToList();
        await Task.WhenAll(tasks);
    }

    public async Task SendMessageToAll(string message)
    {
        var content = $"{message}\n";
        var tasks = clients.Select(client => SendPacket(client, content)).ToList();
        await Task.WhenAll(tasks);
    }

    public async Task Close(PartyPeer target)
    {
        var targets = clients.Where(c => c.guest == target).ToList();
        Debug.Log($"Got from list {targets.Count}");
        foreach (var client in targets)
        {
            Debug.Log("Before event");
            try
            {
                OnDisconnect.Invoke(client.guest);
                client.guest.OnDisconnect.Invoke(client.guest);
            }catch(Exception e)
            {
                Debug.LogError($"Error during disconnect event: {e.Message}");
            }
            Debug.Log("After event");
            await ClosePacket(client);
            Debug.Log($"Client disconnected: {client.guest.name}");
        }
    }

    public List<PartyPeer> GetPeers()
    {
        return clients.Select(c => c.guest).ToList();
    }


    private static char[] CHARACTERS = {
        'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
        'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
        '0','1','2','3','4','5','6','7','8','9',
        '&','_','-','=','+','*'
    };

    string Identifier;

    IPAddress myself;

    // LIFE CYCLE //
    bool isRunning = false;
    void Start()
    {
        if(current!=null)return;

        DontDestroyOnLoad(gameObject);
        current = this;

        var random = new System.Random();
        Identifier = PartyName+new int[]{0,0,0,0,0,0,0,0}.Select(_ => CHARACTERS[random.Next(CHARACTERS.Length)]).Aggregate("", (a,b) => a+b);

        myself = LocalIPAddress();
        Debug.Log($"My IP Address: {myself}");
        
        InitListenTcp();
        InitBroadcast();

        StartCoroutine(ListenTcp());
        BroadcastMyself();

        StartCoroutine(WaitForBroadcast());

        isRunning = true;
    }

    void OnDestroy()
    {
        if(!isRunning) return;

        DestroyListenTcp();
        DestroyBroadcast();
        DestroyTcpLoop();
        current = null;
        isRunning = false;
    }


    // CONNECTION // 
    private List<Client> clients = new();


    // WAIT FOR CONNECTION //
    private TcpListener tcpListener;

    void InitListenTcp()
    {
        tcpListener = new TcpListener(myself, TcpPort);
        tcpListener.Start();
        Log("TCP Listener started");
    }

    void DestroyListenTcp()
    {
        tcpListener.Stop();
        Log("TCP Listener stopped");
    }

    async Task<bool> ListenOnTcp()
    {
        // Connect to a pear
        var connection = await tcpListener.AcceptTcpClientAsync();
        var client = new Client
        {
          client = connection,
          stream = connection.GetStream(),
          end = 0,
          addr = ((IPEndPoint)connection.Client.RemoteEndPoint).Address
        };

        connection.ReceiveTimeout = 5000;
        connection.SendTimeout = 5000;

        client.guest = new PartyPeer();        

        // Check protocol header
        var header = await ReadPacket(client);
        if (header != "party_connect")
        {
            connection.Close();
            return false;
        }
        await SendPacket(client, "party_connect\n");

        // Get name and check double
        client.guest.name = await ReadPacket(client);
        if (clients.Any(c => c.guest.name == client.guest.name))
        {
            connection.Close();
            Debug.Log($"Peer tried to connect with duplicate name: {client.guest.name}");
            return false;
        }
        await SendPacket(client, "ok\n");

        // Send name and check double
        await SendPacket(client, Identifier+"\n");
        var confirmation = await ReadPacket(client);
        if(confirmation!="ok")
        {
            connection.Close();
            return false;
        }

        // Share peer list
        var new_peers = await ReadPacket(client);
        var new_addrs = new_peers.Length==0 ? new() : new_peers.Split(",").Select(str=>IPAddress.Parse(str)).ToList();
        var peers = clients.Count==0 ? "" : clients.Select(c=>c.addr.ToString()).Aggregate((a,b) => a + "," + b);
        await SendPacket(client, peers + "\n");

        // Add to list
        if (client.client.Connected)
        {
            // Now add everything
            clients.Add(client);
            OnConnect.Invoke(client.guest);
            StartCoroutine(TcpLoop(client));
            Debug.Log($"Peer connected: {client.guest.name}");

            // Now try to add the shared peers
            await Task.WhenAll(new_addrs.Select(addr => ConnectTo(addr)));
            
            return true;
        }
        else return false;
    }

    IEnumerator ListenTcp()
    {
        while (true)
        {
            var task = AwaitAsync(ListenOnTcp());
            yield return task;
        }
    }


    // CONNECT TO SOMEONE //
    async Task<bool> ConnectTo(IPAddress ip)
    {
        if(clients.Any(client=>client.addr.Equals(ip)))return false;

        // Connect trade info
        var connection = new TcpClient();
        try
        {
            await connection.ConnectAsync(ip, TcpPort);
        }catch(Exception e)
        {
            Debug.LogError($"Error connecting to {ip}: {e.Message}");
            return false;
        }
        if(!connection.Connected)return false;

        var client = new Client
        {
            client = connection,
            stream = connection.GetStream(),
            addr = ip,
            end = 0,
        };

        connection.ReceiveTimeout = 5000;
        connection.SendTimeout = 5000;

        client.guest = new PartyPeer();    

        // Check protocol header
        await SendPacket(client, "party_connect\n");
        var header = await ReadPacket(client);
        if(header!="party_connect")
        {
            connection.Close();
            return false;
        }

        // Send name and check if no double name
        await SendPacket(client, Identifier + "\n"); // Send name
        var confirmation = await ReadPacket(client);
        if(confirmation!="ok")
        {
            connection.Close();
            return false;
        }

        // Get name and check if no double name
        client.guest.name = await ReadPacket(client);
        if(clients.Any(c => c.guest.name == client.guest.name) || client.guest.name==Identifier)
        {
            connection.Close();
            Debug.Log($"Peer tried to connect with duplicate name: {client.guest.name}");
            return false;
        }
        await SendPacket(client, "ok\n");

        // Share peer list
        var peers = clients.Count==0 ? "" : clients.Select(c=>c.addr.ToString()).Aggregate((a,b) => a + "," + b);
        await SendPacket(client, peers + "\n");
        var new_peers = await ReadPacket(client);
        var new_addrs = new_peers.Length==0 ? new() : new_peers.Split(",").Select(str=>IPAddress.Parse(str)).ToList();

        if (client.client.Connected)
        {
            // Now add everything
            clients.Add(client);
            OnConnect.Invoke(client.guest);
            StartCoroutine(TcpLoop(client));
            Debug.Log($"Peer connected: {client.guest.name}");

            // Now try to add the shared peers
            await Task.WhenAll(new_addrs.Select(addr => ConnectTo(addr)));
            
            return true;
        }
        else return false;
    }


    // TCP LOOP //
    async Task DestroyTcpLoop()
    {
        Debug.Log($"Destroying TCP loop {clients.Count}");
        foreach(var client in clients)
        {
            Debug.Log($"Closing connection with {client.guest.name} {client.client.Connected}");
            if(client.client.Connected) await Close(client.guest);
        }
    }

    IEnumerator TcpLoop(Client client)
    {
        Debug.Log($"[] is connected {client.client.Connected}");
        while (client.client.Connected)
        {
            var task = ReadPacket(client);
            Debug.Log($"Wait for message from {client.guest.name}");
            yield return AwaitAsync(task);
            Debug.Log($"Received message from {client.guest.name}: {task.Result}");
            if (task.Result != null)
            {
                if(task.Result=="CLOSE") client.client.Close();
                else
                {
                    Debug.Log($"Received message from {client.guest.name}: {task.Result}");
                    OnMessage.Invoke(new PartyMessage{peer=client.guest,message=task.Result});
                }
            }
            else
            {
                client.client.Close();
            }
        }
        clients.Remove(client);
        client.guest.OnDisconnect.Invoke(client.guest);
        OnDisconnect.Invoke(client.guest);
        Debug.Log($"Client disconnected: {client.guest.name}");
    }


    // BROADCAST //
    private UdpClient udpClient;

    void InitBroadcast()
    {
        try
        {
            udpClient = new UdpClient(new IPEndPoint(myself, UdpPort));
            udpClient.EnableBroadcast = true;
            Debug.Log($"[NETWORK:{Identifier}] UDP client initialized for broadcasting.");
        }catch(Exception e)
        {
            Debug.LogError($"[NETWORK:{Identifier}] Error initializing UDP client: {e.Message}");
        }
    }

    void DestroyBroadcast()
    {
        if (udpClient != null)
        {
            udpClient.Close();
            udpClient = null;
            Debug.Log($"[NETWORK:{Identifier}] UDP client destroyed.");
        }
    }

    void BroadcastMyself()
    {
        for(var i=0; i<5; i++)
        {
            // Get Ip
            var ip = myself.ToString();
            var bytes = Encoding.ASCII.GetBytes($"broadcast_party;{ip}");

            // Broadcast
            udpClient.Send(bytes, bytes.Length, "255.255.255.255", UdpPort);

            // Simulate broadcast to loopback
            if(ip.ToString().StartsWith("127")) for(var n=1; n<10; n++) udpClient.Send(bytes, bytes.Length, $"127.0.0.{n}", UdpPort);
        }
    }

    IEnumerator WaitForBroadcast()
    {
        while (true)
        {
            var task = udpClient.ReceiveAsync();
            yield return AwaitAsync(task,true);

            try{
                var _ = task.Result;
            }
            catch(Exception e){
                continue;
            }


            if(task.Result.RemoteEndPoint.Address.Equals(myself))continue;
            string msg = Encoding.ASCII.GetString(task.Result.Buffer);

            if (msg!=null && msg.StartsWith("broadcast_party;"))
            {
                var addr_str = msg.Substring("broadcast_party;".Length);
                var addr = IPAddress.Parse(addr_str);
                yield return AwaitAsync(ConnectTo(addr));
            }
        }
    }

    string TryGetPacket(Client client)
    {
        for(var i = client.end_check; i < client.end; i++)
        {
            // End of packet
            if (client.buffer[i] == '\n')
            {
                // Get string
                var str = Encoding.ASCII.GetString(client.buffer, 0, i);
                
                // Put remaining to the start
                var remaining_size = client.end - i - 1;
                for(var y=0; y<remaining_size; y++)
                {
                    client.buffer[y] = client.buffer[i+1+y];
                }
                client.end = remaining_size;
                client.end_check = 0;

                return str;
            }    
        }
        client.end_check = client.end;
        return null;
    }

    async Task<string> ReadPacket(Client client)
    {
        while(true){
            // Check for old message
            var remaining = TryGetPacket(client);
            if(remaining != null) return remaining;

            // Receive
            var start = client.end;
            client.end += await client.stream.ReadAsync(client.buffer, client.end, client.buffer.Length-client.end);
            var length = client.end - start;
            Debug.Log($"Read {client.end - start} bytes from client.");
            
            // Check for new message
            remaining = TryGetPacket(client);
            if(remaining != null) return remaining;

            // Check for out of bounds
            if(client.end == client.buffer.Length-1)
            {
                var str = Encoding.ASCII.GetString(client.buffer, 0, client.end);
                Debug.LogError($"Message too big. {str}");
                client.end = 0;
                return null;
            }

            // If end of connection
            if (!client.client.Connected || length==0)
            {
                return null;
            }
        }
    }

    async Task SendPacket(Client client, string message)
    {
        if (client.client.Connected)
        {
            await client.stream.WriteAsync(Encoding.ASCII.GetBytes(message));
            await client.stream.FlushAsync();
        }
    }

    async Task ClosePacket(Client client)
    {
        Debug.Log($"Closing connection with {client.guest.name}");
        if (client.client.Connected)
        {
            await SendPacket(client, "CLOSE\n");
            client.client.Close();
        }
    }

    IPAddress LocalIPAddress()
    {
        // If editor
        if (Application.isEditor)
        {
            //return IPAddress.Parse("127.0.0.1");
        }

        // Get from args
        var ip_str = GetArg("--myself-ip");
        if (ip_str != null)
        {
            var ip = IPAddress.Parse(ip_str);
            return ip;
        }

        // Auto
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip;
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    private void Log(string msg)
    {
        Debug.Log($"[PARTY] {msg}");
    }

    private static string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }

    public static IEnumerator AwaitAsync(Task task, bool doContinue=false)
    {
        while (!task.IsCompleted)
            yield return null;

        if (task.IsFaulted)
        {
            if(!doContinue)throw task.Exception;
        }
    }
}
