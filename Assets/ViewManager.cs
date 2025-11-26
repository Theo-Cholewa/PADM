using Unity.Netcode;
using UnityEngine;

public class ViewManager : MonoBehaviour
{
    public GameObject tableCamera;
    public GameObject vrPlayerPrefab;

    void Start()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Je suis l'Hôte (Table). J'active la vue de la carte.");
            tableCamera.SetActive(true);
            vrPlayerPrefab.SetActive(false);
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("Je suis un Client (VR). J'active la vue du labyrinthe.");
            tableCamera.SetActive(false);
            vrPlayerPrefab.SetActive(true);
        }
    }
}