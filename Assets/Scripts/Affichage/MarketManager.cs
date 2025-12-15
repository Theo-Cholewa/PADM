using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class MarketManager : MonoBehaviour
{
    [Header("Team Managers")]
    public TeamManager redTeam;
    public TeamManager blueTeam;
    private TeamManager activeTeam;

    [Header("Resource Prices")]
    public int woodPrice = 12;
    public int rockPrice = 23;
    public int chickenPrice = 10;

    [Header("Resource number")]
    public int woodNumber = 100;
    public int rockNumber = 100;
    public int chickenNumber = 100;

    [Header("Upgrade Prices")]
    public int redCannonPrice = 150;
    public int redPiratePrice = 35;
    public int redBarrelPrice = 75;
    public int redShipPrice = 50;
    
    public int blueCannonPrice = 150;
    public int bluePiratePrice = 35;
    public int blueBarrelPrice = 75;
    public int blueShipPrice = 50;
    
    [Header("UI Text References (Prices)")]
    public Text buyWoodPriceText; public Text sellWoodPriceText;
    public Text buyChickenPriceText; public Text sellChickenPriceText;
    public Text buyRockPriceText; public Text sellRockPriceText;
    
    public Text redCannonPriceText1; 
    public Text redPiratePriceText1;
    public Text redBarrelPriceText1; 
    public Text redShipPriceText1; 
    
    public Text blueCannonPriceText1; 
    public Text bluePiratePriceText1; 
    public Text blueBarrelPriceText1; 
    public Text blueShipPriceText1; 

    public Text woodStockText;
    public Text rockStockText;
    public Text chickenStockText;

    [Header("Buttons References")]
    public Button buyWood; public Button sellWood;
    public Button buyChicken; public Button sellChicken;
    public Button buyRock; public Button sellRock;    
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

    [Header("Image Buttons References")]
    public RawImage  upCannonRed;
    public RawImage  upPirateRed;
    public RawImage  upBarrelRed;
    public RawImage  upShipRed;
    public RawImage  upCannonBlue;
    public RawImage  upPirateBlue;
    public RawImage  upBarrelBlue;
    public RawImage  upShipBlue;
    public Texture  GreySprite;
    public Texture  GreenSprite;



    void Start()
    {
        baseWoodPrice = woodPrice;
        baseRockPrice = rockPrice;
        baseChickenPrice = chickenPrice;
        activeTeam = null;

        UpdatePriceUI();
        SetupButtons();
        StartCoroutine(EconomicCycleRoutine());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (activeTeam == blueTeam)
            {
                activeTeam = null;
                UpdateTeamVisuals(null);
                Debug.Log(">>> Aucune équipe n'est maintenant au marché !");
            }
            else
            {
                activeTeam = blueTeam;
                UpdateTeamVisuals(blueTeam);
                Debug.Log(">>> La Team BLUE est maintenant au marché !");
            }
            
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            if (activeTeam == redTeam)
            {
                activeTeam = null;
                UpdateTeamVisuals(null);
                Debug.Log(">>> Aucune équipe n'est maintenant au marché !");

            }
            else
            {
                activeTeam = redTeam;
                UpdateTeamVisuals(redTeam);
                Debug.Log(">>> La Team RED est maintenant au marché !");
            }
        }
    }

    void UpdateTeamVisuals(TeamManager currentTeam)
    {
        if (currentTeam == redTeam)
        {
            if (GreySprite != null && GreenSprite != null)
                upCannonRed.texture = GreenSprite;
                upBarrelRed.texture = GreenSprite;
                upPirateRed.texture = GreenSprite;
                upShipRed.texture = GreenSprite;

                upCannonBlue.texture = GreySprite;
                upBarrelBlue.texture = GreySprite;
                upPirateBlue.texture = GreySprite;
                upShipBlue.texture = GreySprite;
        }
        else if (currentTeam == blueTeam)
        {
            if (GreySprite != null && GreenSprite != null)
                upCannonBlue.texture = GreenSprite;
                upBarrelBlue.texture = GreenSprite;
                upPirateBlue.texture = GreenSprite;
                upShipBlue.texture = GreenSprite;

                upCannonRed.texture = GreySprite;
                upBarrelRed.texture = GreySprite;    
                upPirateRed.texture = GreySprite;
                upShipRed.texture = GreySprite;
        }
        else if (currentTeam == null)
        {
            if (GreySprite != null)
            {
                upCannonRed.texture = GreySprite;
                upBarrelRed.texture = GreySprite;
                upPirateRed.texture = GreySprite;
                upShipRed.texture = GreySprite;

                upCannonBlue.texture = GreySprite;
                upBarrelBlue.texture = GreySprite;
                upPirateBlue.texture = GreySprite;
                upShipBlue.texture = GreySprite;
            }
        }
    }


    void UpdatePriceUI()
    {
        string g = "g";
        string p1 = "(";
        string p2 = ")";

        SetText(p1 + woodPrice + g + p2, buyWoodPriceText);
        int sellWoodPrice = Mathf.CeilToInt(woodPrice * 0.8f);
        SetText(p1 + sellWoodPrice + g + p2, sellWoodPriceText);
        SetText(p1 + chickenPrice + g + p2, buyChickenPriceText);
        int sellChickenPrice = Mathf.CeilToInt(chickenPrice * 0.8f);
        SetText(p1 + sellChickenPrice + g + p2, sellChickenPriceText);
        SetText(p1 + rockPrice + g + p2, buyRockPriceText);
        int sellRockPrice = Mathf.CeilToInt(rockPrice * 0.8f);
        SetText(p1 + sellRockPrice + g + p2, sellRockPriceText);

        SetText(p1 + redCannonPrice + g + p2, redCannonPriceText1);
        SetText(p1 + redPiratePrice + g + p2, redPiratePriceText1);
        SetText(p1 + redBarrelPrice + g + p2, redBarrelPriceText1);
        SetText(p1 + redShipPrice + g + p2, redShipPriceText1);

        SetText(p1 + blueCannonPrice + g + p2, blueCannonPriceText1);
        SetText(p1 + bluePiratePrice + g + p2, bluePiratePriceText1);
        SetText(p1 + blueBarrelPrice + g + p2, blueBarrelPriceText1);
        SetText(p1 + blueShipPrice + g + p2, blueShipPriceText1);
    }

void SetupButtons()
    {
        buyWood.onClick.AddListener(() => BuyResource(activeTeam, ResourceType.Wood, woodPrice));
        sellWood.onClick.AddListener(() => SellResource(activeTeam, ResourceType.Wood, woodPrice));

        buyChicken.onClick.AddListener(() => BuyResource(activeTeam, ResourceType.Chicken, chickenPrice));
        sellChicken.onClick.AddListener(() => SellResource(activeTeam, ResourceType.Chicken, chickenPrice));

        buyRock.onClick.AddListener(() => BuyResource(activeTeam, ResourceType.Rock, rockPrice));
        sellRock.onClick.AddListener(() => SellResource(activeTeam, ResourceType.Rock, rockPrice));

        UpgradeCannonRedTeam.onClick.AddListener(() => BuyUpgrade(redTeam, ResourceType.Cannon, redCannonPrice));
        UpgradePirateRedTeam.onClick.AddListener(() => BuyUpgrade(redTeam, ResourceType.Pirate, redPiratePrice));
        UpgradeBarrelRedTeam.onClick.AddListener(() => BuyUpgrade(redTeam, ResourceType.Barrel, redBarrelPrice));
        UpgradeShipRedTeam.onClick.AddListener(() => BuyUpgrade(redTeam, ResourceType.Ship, redShipPrice));

        UpgradeCannonBlueTeam.onClick.AddListener(() => BuyUpgrade(blueTeam, ResourceType.Cannon, blueCannonPrice));
        UpgradePirateBlueTeam.onClick.AddListener(() => BuyUpgrade(blueTeam, ResourceType.Pirate, bluePiratePrice));
        UpgradeBarrelBlueTeam.onClick.AddListener(() => BuyUpgrade(blueTeam, ResourceType.Barrel, blueBarrelPrice));
        UpgradeShipBlueTeam.onClick.AddListener(() => BuyUpgrade(blueTeam, ResourceType.Ship, blueShipPrice));
    }


public void BuyResource(TeamManager team, ResourceType type, int price)
    {
        if (team == null) {
            Debug.LogWarning("Aucune équipe n'est active au marché ! Appuyez sur R ou B.");
            return;
        }

        if (team.gold >= price)
        {
            team.ModifyResource(ResourceType.Gold, -price);
            team.ModifyResource(type, 1);
            
            // --- AJOUT DE L'ANIMATION ICI ---
            team.AnimateResource(type);
            // --------------------------------

            Debug.Log(team.team + " bought " + type);
        }
        else
        {
            Debug.Log(team.team + " : Not enough Gold!");
        }
    }

    public void SellResource(TeamManager team, ResourceType type, int price)
    {
        if (team == null) {
            Debug.LogWarning("Aucune équipe n'est active au marché ! Appuyez sur R ou B.");
            return;
        }

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
            team.ModifyResource(ResourceType.Gold, sellPrice);

            // --- AJOUT DE L'ANIMATION ICI ---
            team.AnimateResource(type);
            // --------------------------------

            Debug.Log(team.team + " sold " + type);
        }
        else
        {
            Debug.Log(team.team + " : Not enough resources to sell!");
        }
    }

    public void BuyUpgrade(TeamManager team, ResourceType type, int price)
    {
        if (team == null) {
            Debug.LogWarning("Aucune équipe n'est active au marché ! Appuyez sur R ou B.");
            return;
        }
        if (team != activeTeam) {
            Debug.LogWarning(team.team + " ne peut pas acheter d'améliorations car ce n'est pas son tour au marché !");
            return;
        }

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
            Debug.Log(team.team + " : " + type + " est déjà au niveau Max (5) !");
            return;
        }

        if (team.gold >= price)
        {
            team.ModifyResource(ResourceType.Gold, -price);
            team.ModifyResource(type, 1);
            Debug.Log(team.team + " upgraded " + type + " to level " + (currentLevel + 1));
        }
        else
        {
            Debug.Log(team.team + " : Not enough Gold for upgrade!");
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

    void TriggerRandomBoom()
    {
        Debug.Log("--- ECONOMIC BOOM STARTED ---");
        bool modifyTwoResources = (Random.value > 0.5f);
        List<ResourceType> availableTypes = new List<ResourceType> { ResourceType.Wood, ResourceType.Rock, ResourceType.Chicken };

        if (modifyTwoResources)
        {
            int index1 = Random.Range(0, availableTypes.Count);
            ResourceType type1 = availableTypes[index1];
            availableTypes.RemoveAt(index1);

            int index2 = Random.Range(0, availableTypes.Count);
            ResourceType type2 = availableTypes[index2];
            bool firstIsUp = (Random.value > 0.5f);
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
                int sellWoodPrice = Mathf.CeilToInt(woodPrice * 0.8f);
                UpdateTextColor(woodPrice, baseWoodPrice, buyWoodPriceText);
                UpdateTextColor(sellWoodPrice, baseWoodPrice, sellWoodPriceText);
                break;

            case ResourceType.Rock:
                rockPrice = Mathf.CeilToInt(baseRockPrice * multiplier);
                Debug.Log($"PIERRE : {baseRockPrice} -> {rockPrice} (x{multiplier:F2})");
                int sellRockPrice = Mathf.CeilToInt(rockPrice * 0.8f);
                UpdateTextColor(rockPrice, baseRockPrice, buyRockPriceText);
                UpdateTextColor(sellRockPrice, baseRockPrice, sellRockPriceText);
                break;

            case ResourceType.Chicken:
                chickenPrice = Mathf.CeilToInt(baseChickenPrice * multiplier);
                Debug.Log($"POULET : {baseChickenPrice} -> {chickenPrice} (x{multiplier:F2})");
                int sellChickenPrice = Mathf.CeilToInt(chickenPrice * 0.8f);
                UpdateTextColor(chickenPrice, baseChickenPrice, buyChickenPriceText);
                UpdateTextColor(sellChickenPrice, baseChickenPrice, sellChickenPriceText);
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
        int sellWoodPrice = Mathf.CeilToInt(woodPrice * 0.8f);
        rockPrice = baseRockPrice;
        int sellRockPrice = Mathf.CeilToInt(rockPrice * 0.8f);
        chickenPrice = baseChickenPrice;
        int sellChickenPrice = Mathf.CeilToInt(chickenPrice * 0.8f);

        UpdateTextColor(woodPrice, baseWoodPrice, buyWoodPriceText);
        UpdateTextColor(sellWoodPrice, baseWoodPrice, sellWoodPriceText);
        UpdateTextColor(rockPrice, baseRockPrice, buyRockPriceText);
        UpdateTextColor(sellRockPrice, baseRockPrice, sellRockPriceText);
        UpdateTextColor(chickenPrice, baseChickenPrice, buyChickenPriceText);
        UpdateTextColor(sellChickenPrice, baseChickenPrice, sellChickenPriceText);

        UpdatePriceUI();
    }
}