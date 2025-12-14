using Unity.Netcode;
using UnityEngine;

public class HostManager : MonoBehaviour
{
    public static HostManager Instance;

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
        StartHostMode();
    }

    public void StartHostMode()
    {
        if (NetworkManager.Singleton.IsHost) return;

        NetworkManager.Singleton.StartHost();
        Debug.Log("Serveur (Hôte) persistant démarré ! En attente du casque VR...");
    }
}

// Attention : Puisque le NetworkManager gère maintenant la session,  ne  PLUS changer de scène avec le SceneManager normal d'Unity.
// Pour changer de scène en réseau (et dire à tous les clients de changer aussi), appeler :
// NetworkManager.Singleton.SceneManager.LoadScene("NomDeVotreScene", LoadSceneMode.Single);
// ...et non l'ancien SceneManager.LoadScene("NomDeVotreScene");