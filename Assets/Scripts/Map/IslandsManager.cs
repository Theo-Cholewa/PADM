using UnityEngine;

public class IslandsManager : MonoBehaviour
{
    [Header("Références aux bateaux")]
    public ShipController blueShip;
    public ShipController redShip;

    [Header("Références aux îles")]
    public Island[] islands; // assigner les îles dans l’inspecteur

    [Header("Touches actions")]
    public string blueShipSelector = "b";
    public string redShipSelector = "n";
    public string foodKey = "x";
    public string woodKey = "c";
    public string stoneKey = "v";

    private string currentShip = "";  // "blue" ou "red"
    private int currentIsland = -1;   // numéro de l’île sélectionnée

    void Update()
    {
        // 1️⃣ Sélection du bateau
        if (Input.GetKeyDown(blueShipSelector))
        {
            currentShip = "blue";
            Debug.Log("→ Bateau bleu sélectionné");
        }
        else if (Input.GetKeyDown(redShipSelector))
        {
            currentShip = "red";
            Debug.Log("→ Bateau rouge sélectionné");
        }

        // 2️⃣ Sélection de l'île (1–9)
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                currentIsland = i;
                Debug.Log($"→ Île {i} sélectionnée");
            }
        }

        // 3️⃣ Sélection de la ressource
        if (Input.GetKeyDown(foodKey))
            TryGiveResource("food");
        else if (Input.GetKeyDown(woodKey))
            TryGiveResource("wood");
        else if (Input.GetKeyDown(stoneKey))
            TryGiveResource("stone");
    }

    void TryGiveResource(string resourceType)
    {
        if (currentShip == "" || currentIsland == -1)
        {
            Debug.LogWarning("⚠ Séquence incomplète (il faut un bateau et une île avant la ressource)");
            return;
        }

        Island targetIsland = GetIslandByID(currentIsland);
        if (targetIsland == null)
        {
            Debug.LogWarning($"⚠ Aucune île avec l’ID {currentIsland}");
            return;
        }

        if (!targetIsland.HasResource(resourceType))
        {
            Debug.Log($"❌ L'île {targetIsland.islandID} ne possède pas de {resourceType} !");
            return;
        }

        ShipController targetShip = (currentShip == "blue") ? blueShip : redShip;
        ShipData data = targetShip.GetComponent<ShipData>();

        int amount = 10;
        data.AddResource(resourceType, amount);

        Debug.Log($"✅ {currentShip.ToUpper()} gagne {amount} de {resourceType} depuis l'île {targetIsland.islandID}");

        // On retire la ressource de l'île
        targetIsland.CollectResource(resourceType);

        currentIsland = -1;
        currentShip = "";
    }


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