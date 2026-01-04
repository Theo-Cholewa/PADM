

using UnityEngine;

public class Shop : MonoBehaviour, IslandBehaviour
{
    public void Dock(ShipController ship)
    {
        ship.ressources.AskOpenShop();
    }

    public void Undock(ShipController ship)
    {
        ship.ressources.AskCloseShop();
    }
}