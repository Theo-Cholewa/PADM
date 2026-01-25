using UnityEngine;

public class UdpLogger : MonoBehaviour
{
    public UdpPeer udp;

    void Start()
    {
        if (udp == null) udp = GetComponent<UdpPeer>();

        udp.OnUdpMessage += OnMessage;
    }

    void OnDestroy()
    {
        if (udp != null)
            udp.OnUdpMessage -= OnMessage;
    }

    private void OnMessage(string msg)
    {
        // msg ressemble à: "Phone: TEAM=RED"
        Debug.Log("[SERVER RX] " + msg);
    }
}
