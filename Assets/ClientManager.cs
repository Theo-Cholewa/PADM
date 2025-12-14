using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;

public class ClientManager : MonoBehaviour
{
    public static ClientManager Instance;

    [Header("Connexion")]
    [Tooltip("L'adresse IP du PC qui est l'Hôte (Serveur)")]
    public string hostIpAddress = "10.212.105.141"; // METTEZ L'IP DE VOTRE PC ICI

    [Tooltip("Le port de connexion (doit être le même que l'Hôte)")]
    public ushort port = 7777;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        if (HostManager.Instance != null && NetworkManager.Singleton.IsHost)
        {
            Destroy(this.gameObject);
            return;
        }

        ConfigureAndStartClient();
    }

    public void ConfigureAndStartClient()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost) return;

        Debug.Log($"Client : Configuration de la connexion à {hostIpAddress}:{port}...");

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            transport.SetConnectionData(hostIpAddress, port);
        }
        else
        {
            Debug.LogError("Pas de UnityTransport trouvé sur le NetworkManager !");
            return;
        }

        NetworkManager.Singleton.StartClient();
        Debug.Log("Client démarré ! Tentative de connexion à l'Hôte...");
    }
}