using UnityEngine;

public class IslandsManager : MonoBehaviour
{
    [Header("Références aux bateaux")]
    public ShipController blueShip;
    public ShipController redShip;

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
        // --- Détection des touches ---

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

        // 2️⃣ Sélection de l'île (chiffre 1 à 9)
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
        {
            TryGiveResource("food");
        }
        else if (Input.GetKeyDown(woodKey))
        {
            TryGiveResource("wood");
        }
        else if (Input.GetKeyDown(stoneKey))
        {
            TryGiveResource("stone");
        }
    }

    void TryGiveResource(string resourceType)
    {
        if (currentShip == "" || currentIsland == -1)
        {
            Debug.LogWarning("⚠ Séquence incomplète (il faut un bateau et une île avant la ressource)");
            return;
        }

        ShipController targetShip = (currentShip == "blue") ? blueShip : redShip;
        ShipData data = targetShip.GetComponent<ShipData>();

        int amount = 10; // quantité fixe, tu pourras changer

        data.AddResource(resourceType, amount);
        Debug.Log($"✅ {currentShip.ToUpper()} gagne {amount} de {resourceType} depuis l’île {currentIsland}");

        // Reset de la séquence après l’action
        currentIsland = -1;
        currentShip = "";
    }
}
