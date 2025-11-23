using UnityEngine;

public class IslandsManager : MonoBehaviour
{
    [Header("Références aux bateaux")]
    public ShipController blueShip;
    public ShipController redShip;

    [Header("Références aux îles")]
    public Island[] islands; // assigner les îles dans l’inspecteur

    private string currentShip = "";  // "blue" ou "red"
    private int currentIsland = -1;   // numéro de l’île sélectionné


    Island GetIslandByID(int id)
    {
        foreach (var island in islands)
        {
            if (island.islandID == id)
                return island;
        }
        return null;
    }
}