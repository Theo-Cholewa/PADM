using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class MarketManager : MonoBehaviour
{
    [Header("Team Managers")]
    public TeamManager redTeam;
    public TeamManager blueTeam;

    [Header("Resource Prices")]
    public int woodPrice = 12;
    public int rockPrice = 23;
    public int chickenPrice = 10;

    [Header("Upgrades Prices")]
    public int redCannonPrice = 150;
    public int redPiratePrice = 35;
    public int redBarrelPrice = 75;
    public int redShipPrice = 50;
    
    public int blueCannonPrice = 150;
    public int bluePiratePrice = 35;
    public int blueBarrelPrice = 75;
    public int blueShipPrice = 50;
    
    [Header("UI Text References (Prices)")]
    public Text woodPrice1; public Text woodPrice2; public Text woodPrice3; public Text woodPrice4;
    public Text chickenPrice1; public Text chickenPrice2; public Text chickenPrice3; public Text chickenPrice4;
    public Text rockPrice1; public Text rockPrice2; public Text rockPrice3; public Text rockPrice4;
    
    public Text redCannonPriceText1; public Text redCannonPriceText2;
    public Text redPiratePriceText1; public Text redPiratePriceText2;
    public Text redBarrelPriceText1; public Text redBarrelPriceText2;
    public Text redShipPriceText1; public Text redShipPriceText2;
    
    public Text blueCannonPriceText1; public Text blueCannonPriceText2;
    public Text bluePiratePriceText1; public Text bluePiratePriceText2;
    public Text blueBarrelPriceText1; public Text blueBarrelPriceText2;
    public Text blueShipPriceText1; public Text blueShipPriceText2;

    [Header("Buttons References")]
    public Button buyWoodRedTeam; public Button sellWoodRedTeam;
    public Button buyWoodBlueTeam; public Button sellWoodBlueTeam;
    
    public Button buyChickenRedTeam; public Button sellChickenRedTeam;
    public Button buyChickenBlueTeam; public Button sellChickenBlueTeam;
    
    public Button buyRockRedTeam; public Button sellRockRedTeam;
    public Button buyRockBlueTeam; public Button sellRockBlueTeam;
    
    public Button UpgradeCannonRedTeam; public Button UpgradeCannonBlueTeam;
    public Button UpgradePirateRedTeam; public Button UpgradePirateBlueTeam;
    public Button UpgradeBarrelRedTeam; public Button UpgradeBarrelBlueTeam;
    public Button UpgradeShipRedTeam; public Button UpgradeShipBlueTeam;

    [Header("Economic System")]
    public float boomDuration = 45f;
    public float minTimeBetweenBooms = 20f;
    public float maxTimeBetweenBooms = 60f;
    private int baseWoodPrice;
    private int baseRockPrice;
    private int baseChickenPrice;


    void Start()
    {
        baseWoodPrice = woodPrice;
        baseRockPrice = rockPrice;
        baseChickenPrice = chickenPrice;

        UpdatePriceUI();
        SetupButtons();

        StartCoroutine(EconomicCycleRoutine());
    }

    // --- SETUP ---

    void UpdatePriceUI()
    {
        string g = "g";

        SetText(woodPrice + g, woodPrice1, woodPrice2, woodPrice3, woodPrice4);
        SetText(chickenPrice + g, chickenPrice1, chickenPrice2, chickenPrice3, chickenPrice4);
        SetText(rockPrice + g, rockPrice1, rockPrice2, rockPrice3, rockPrice4);

        SetText(redCannonPrice + g, redCannonPriceText1, redCannonPriceText2);
        SetText(redPiratePrice + g, redPiratePriceText1, redPiratePriceText2);
        SetText(redBarrelPrice + g, redBarrelPriceText1, redBarrelPriceText2);
        SetText(redShipPrice + g, redShipPriceText1, redShipPriceText2);

        SetText(blueCannonPrice + g, blueCannonPriceText1, blueCannonPriceText2);
        SetText(bluePiratePrice + g, bluePiratePriceText1, bluePiratePriceText2);
        SetText(blueBarrelPrice + g, blueBarrelPriceText1, blueBarrelPriceText2);
        SetText(blueShipPrice + g, blueShipPriceText1, blueShipPriceText2);
    }

    void SetupButtons()
    {
        buyWoodRedTeam.onClick.AddListener(() => BuyResource(redTeam, ResourceType.Wood, woodPrice));
        sellWoodRedTeam.onClick.AddListener(() => SellResource(redTeam, ResourceType.Wood, woodPrice));

        buyChickenRedTeam.onClick.AddListener(() => BuyResource(redTeam, ResourceType.Chicken, chickenPrice));
        sellChickenRedTeam.onClick.AddListener(() => SellResource(redTeam, ResourceType.Chicken, chickenPrice));

        buyRockRedTeam.onClick.AddListener(() => BuyResource(redTeam, ResourceType.Rock, rockPrice));
        sellRockRedTeam.onClick.AddListener(() => SellResource(redTeam, ResourceType.Rock, rockPrice));

        UpgradeCannonRedTeam.onClick.AddListener(() => BuyUpgrade(redTeam, ResourceType.Cannon, redCannonPrice));
        UpgradePirateRedTeam.onClick.AddListener(() => BuyUpgrade(redTeam, ResourceType.Pirate, redPiratePrice));
        UpgradeBarrelRedTeam.onClick.AddListener(() => BuyUpgrade(redTeam, ResourceType.Barrel, redBarrelPrice));
        UpgradeShipRedTeam.onClick.AddListener(() => BuyUpgrade(redTeam, ResourceType.Ship, redShipPrice));

        buyWoodBlueTeam.onClick.AddListener(() => BuyResource(blueTeam, ResourceType.Wood, woodPrice));
        sellWoodBlueTeam.onClick.AddListener(() => SellResource(blueTeam, ResourceType.Wood, woodPrice));

        buyChickenBlueTeam.onClick.AddListener(() => BuyResource(blueTeam, ResourceType.Chicken, chickenPrice));
        sellChickenBlueTeam.onClick.AddListener(() => SellResource(blueTeam, ResourceType.Chicken, chickenPrice));

        buyRockBlueTeam.onClick.AddListener(() => BuyResource(blueTeam, ResourceType.Rock, rockPrice));
        sellRockBlueTeam.onClick.AddListener(() => SellResource(blueTeam, ResourceType.Rock, rockPrice));

        UpgradeCannonBlueTeam.onClick.AddListener(() => BuyUpgrade(blueTeam, ResourceType.Cannon, blueCannonPrice));
        UpgradePirateBlueTeam.onClick.AddListener(() => BuyUpgrade(blueTeam, ResourceType.Pirate, bluePiratePrice));
        UpgradeBarrelBlueTeam.onClick.AddListener(() => BuyUpgrade(blueTeam, ResourceType.Barrel, blueBarrelPrice));
        UpgradeShipBlueTeam.onClick.AddListener(() => BuyUpgrade(blueTeam, ResourceType.Ship, blueShipPrice));
    }


    public void BuyResource(TeamManager team, ResourceType type, int price)
    {
        if (team.gold >= price)
        {
            team.ModifyGold(-price);
            team.ModifyResource(type, 1);
            Debug.Log(team.teamName + " bought " + type);
        }
        else
        {
            Debug.Log(team.teamName + " : Not enough Gold!");
        }
    }

    public void SellResource(TeamManager team, ResourceType type, int price)
    {
        bool hasResource = false;
        switch (type)
        {
            case ResourceType.Wood: hasResource = team.wood > 0; break;
            case ResourceType.Rock: hasResource = team.rock > 0; break;
            case ResourceType.Chicken: hasResource = team.chicken > 0; break;
        }

        if (hasResource)
        {
            int sellPrice = Mathf.CeilToInt(price * 0.75f); 
            team.ModifyResource(type, -1);
            team.ModifyGold(sellPrice);
            Debug.Log(team.teamName + " sold " + type);
        }
        else
        {
            Debug.Log(team.teamName + " : Not enough resources to sell!");
        }
    }

    public void BuyUpgrade(TeamManager team, ResourceType type, int price)
    {
        int currentLevel = 0;
        
        switch (type)
        {
            case ResourceType.Cannon: currentLevel = team.cannonLevel; break;
            case ResourceType.Pirate: currentLevel = team.pirateLevel; break;
            case ResourceType.Barrel: currentLevel = team.barrelLevel; break;
            case ResourceType.Ship:   currentLevel = team.shipLevel;   break;
            default: return;
        }

        if (currentLevel >= 5)
        {
            Debug.Log(team.teamName + " : " + type + " est déjà au niveau Max (5) !");
            return;
        }

        if (team.gold >= price)
        {
            team.ModifyGold(-price);
            team.ModifyResource(type, 1);
            Debug.Log(team.teamName + " upgraded " + type + " to level " + (currentLevel + 1));
        }
        else
        {
            Debug.Log(team.teamName + " : Not enough Gold for upgrade!");
        }
    }

    void SetText(string text, params Text[] textComponents)
    {
        foreach(Text t in textComponents)
        {
            if(t != null) t.text = text;
        }
    }

    IEnumerator EconomicCycleRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minTimeBetweenBooms, maxTimeBetweenBooms);
            yield return new WaitForSeconds(waitTime);

            TriggerRandomBoom();

            yield return new WaitForSeconds(boomDuration);

            ResetPrices();
        }
    }

// --- NOUVELLE LOGIQUE DU BOOM ---

    void TriggerRandomBoom()
    {
        Debug.Log("--- ECONOMIC BOOM STARTED ---");

        // Pile ou face : modifie-t-on 1 ou 2 ressources ?
        bool modifyTwoResources = (Random.value > 0.5f);

        // Liste des types disponibles
        List<ResourceType> availableTypes = new List<ResourceType> { ResourceType.Wood, ResourceType.Rock, ResourceType.Chicken };

        if (modifyTwoResources)
        {
            // --- CAS : 2 RESSOURCES (1 UP, 1 DOWN) ---
            
            // 1. On tire 2 ressources au hasard
            int index1 = Random.Range(0, availableTypes.Count);
            ResourceType type1 = availableTypes[index1];
            availableTypes.RemoveAt(index1); // On retire pour ne pas la repiocher

            int index2 = Random.Range(0, availableTypes.Count);
            ResourceType type2 = availableTypes[index2];

            // 2. On décide qui monte et qui descend
            bool firstIsUp = (Random.value > 0.5f);

            // 3. Génération des multiplicateurs
            // UP : Entre x1.2 et x2.5
            // DOWN : Entre x0.4 et x0.8
            float mult1 = firstIsUp ? Random.Range(1.2f, 2.5f) : Random.Range(0.4f, 0.8f);
            float mult2 = firstIsUp ? Random.Range(0.4f, 0.8f) : Random.Range(1.2f, 2.5f);

            ApplyPriceModification(type1, mult1);
            ApplyPriceModification(type2, mult2);
        }
        else
        {           
            int index = Random.Range(0, availableTypes.Count);
            ResourceType type = availableTypes[index];

            float randomMult = Random.Range(0.5f, 2.5f);
            
            ApplyPriceModification(type, randomMult);
        }

        UpdatePriceUI();
    }

    void ApplyPriceModification(ResourceType type, float multiplier)
    {
        switch (type)
        {
            case ResourceType.Wood:
                woodPrice = Mathf.CeilToInt(baseWoodPrice * multiplier);
                Debug.Log($"BOIS : {baseWoodPrice} -> {woodPrice} (x{multiplier:F2})");
                UpdateTextColor(woodPrice, baseWoodPrice, woodPrice1, woodPrice2, woodPrice3, woodPrice4);
                break;

            case ResourceType.Rock:
                rockPrice = Mathf.CeilToInt(baseRockPrice * multiplier);
                Debug.Log($"PIERRE : {baseRockPrice} -> {rockPrice} (x{multiplier:F2})");
                UpdateTextColor(rockPrice, baseRockPrice, rockPrice1, rockPrice2, rockPrice3, rockPrice4);
                break;

            case ResourceType.Chicken:
                chickenPrice = Mathf.CeilToInt(baseChickenPrice * multiplier);
                Debug.Log($"POULET : {baseChickenPrice} -> {chickenPrice} (x{multiplier:F2})");
                UpdateTextColor(chickenPrice, baseChickenPrice, chickenPrice1, chickenPrice2, chickenPrice3, chickenPrice4);
                break;
        }
    }

    void UpdateTextColor(int currentPrice, int basePrice, params Text[] texts)
    {
        Color targetColor = Color.black;
        Color darkGreen = new Color(0f, 0.6f, 0f);

        if (currentPrice > basePrice) 
            targetColor = darkGreen;
        else if (currentPrice < basePrice) 
            targetColor = Color.red;

        foreach (Text t in texts)
        {
            if (t != null) t.color = targetColor;
        }
    }

void ResetPrices()
    {
        Debug.Log("--- FIN DU BOOM ECONOMIQUE ---");
        
        woodPrice = baseWoodPrice;
        rockPrice = baseRockPrice;
        chickenPrice = baseChickenPrice;

        UpdateTextColor(woodPrice, baseWoodPrice, woodPrice1, woodPrice2, woodPrice3, woodPrice4);
        UpdateTextColor(rockPrice, baseRockPrice, rockPrice1, rockPrice2, rockPrice3, rockPrice4);
        UpdateTextColor(chickenPrice, baseChickenPrice, chickenPrice1, chickenPrice2, chickenPrice3, chickenPrice4);

        UpdatePriceUI();
    }
}