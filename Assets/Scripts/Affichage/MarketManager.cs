using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 


[System.Serializable]
public class MarketManagerUpgradeButton
{
    public Text Price;
    public Button Button;
    public RawImage Image;

    public void SetActive(MarketManager market, bool isActive)
    {
        if (isActive)
        {
            Image.texture = market.GreenSprite;
            Button.enabled = true;
        }
        else
        {
            Image.texture = market.GreySprite;
            Button.enabled = false;
        }
    }

    public void SetPrice(MarketManager market, int price)
    {
        market.SetText("(" + price + "g)", Price);
    }

    public Button.ButtonClickedEvent OnClick => Button.onClick;
}

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

    public Text woodStockText;
    public Text rockStockText;
    public Text chickenStockText;

    [Header("Market Buttons")]
    public MarketManagerUpgradeButton RedCanonUpdate;
    public MarketManagerUpgradeButton RedPirateUpdate;
    public MarketManagerUpgradeButton RedBarrelUpdate;
    public MarketManagerUpgradeButton RedShipUpdate;

    public MarketManagerUpgradeButton BlueCanonUpdate;
    public MarketManagerUpgradeButton BluePirateUpdate;
    public MarketManagerUpgradeButton BlueBarrelUpdate;
    public MarketManagerUpgradeButton BlueShipUpdate;

    public Button buyWood; public Button sellWood;
    public Button buyChicken; public Button sellChicken;
    public Button buyRock; public Button sellRock;    

    [Header("Economic System")]
    public float boomDuration = 45f;
    public float minTimeBetweenBooms = 20f;
    public float maxTimeBetweenBooms = 60f;
    private int baseWoodPrice;
    private int baseRockPrice;
    private int baseChickenPrice;

    [Header("Image Buttons References")]
    public Texture  GreySprite;
    public Texture  GreenSprite;

    public GameObject MarketHider;

    public PartyTools.ValueServer<GameStats> stats;

    void Start()
    {
        baseWoodPrice = woodPrice;
        baseRockPrice = rockPrice;
        baseChickenPrice = chickenPrice;
        activeTeam = null;

        UpdatePriceUI();
        SetupButtons();
        StartCoroutine(EconomicCycleRoutine());
        Party.current.OnMessage.AddListener(OnMessage);

        stats = new(
            Party.current,
            "game_stats",
            new GameStats{
                IsInFight = false,
                Winner = null,
            },
            v => JsonUtility.ToJson(v)
        );
    }

    void OnDestroy()
    {
        Party.current.OnMessage.RemoveListener(OnMessage);
        stats.Dispose();
    }

    public void SetActiveTeam(Team team)
    {
        TeamManager manager = null;
        if(team==Team.BLUE) manager = blueTeam;
        else if(team==Team.RED) manager = redTeam;

        activeTeam = manager;
        UpdateTeamVisuals(manager);
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
        var isRed = currentTeam == redTeam;
        RedCanonUpdate.SetActive(this, isRed);
        RedPirateUpdate.SetActive(this, isRed);
        RedBarrelUpdate.SetActive(this, isRed);
        RedShipUpdate.SetActive(this, isRed);

        var isBlue = currentTeam == blueTeam;
        BlueCanonUpdate.SetActive(this, isBlue);
        BluePirateUpdate.SetActive(this, isBlue);
        BlueBarrelUpdate.SetActive(this, isBlue);
        BlueShipUpdate.SetActive(this, isBlue);

        var isNothing = currentTeam == null;
        MarketHider.SetActive(isNothing);
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

        RedCanonUpdate.SetPrice(this, redCannonPrice);
        RedPirateUpdate.SetPrice(this, redPiratePrice);
        RedBarrelUpdate.SetPrice(this, redBarrelPrice);
        RedShipUpdate.SetPrice(this, redShipPrice);

        BlueCanonUpdate.SetPrice(this, blueCannonPrice);
        BluePirateUpdate.SetPrice(this, bluePiratePrice);
        BlueBarrelUpdate.SetPrice(this, blueBarrelPrice);
        BlueShipUpdate.SetPrice(this, blueShipPrice);
    }

    void UpdateStockUI()
    {
        SetText("Stock: " + woodNumber.ToString(), woodStockText);
        SetText("Stock: " + rockNumber.ToString(), rockStockText);
        SetText("Stock: " + chickenNumber.ToString(), chickenStockText);
    }

    void SetupButtons()
    {
        buyWood.onClick.AddListener(() => BuyResource(activeTeam, RessourceType.Wood, woodPrice));
        sellWood.onClick.AddListener(() => SellResource(activeTeam, RessourceType.Wood, woodPrice));
    
        buyChicken.onClick.AddListener(() => BuyResource(activeTeam, RessourceType.Chicken, chickenPrice));
        sellChicken.onClick.AddListener(() => SellResource(activeTeam, RessourceType.Chicken, chickenPrice));

        buyRock.onClick.AddListener(() => BuyResource(activeTeam, RessourceType.Rock, rockPrice));
        sellRock.onClick.AddListener(() => SellResource(activeTeam, RessourceType.Rock, rockPrice));

        RedCanonUpdate.OnClick.AddListener(() => BuyUpgrade(redTeam, RessourceType.Cannon, redCannonPrice));
        RedPirateUpdate.OnClick.AddListener(() => BuyUpgrade(redTeam, RessourceType.Pirate, redPiratePrice));
        RedBarrelUpdate.OnClick.AddListener(() => BuyUpgrade(redTeam, RessourceType.Barrel, redBarrelPrice));
        RedShipUpdate.OnClick.AddListener(() => BuyUpgrade(redTeam, RessourceType.Ship, redShipPrice));

        BlueCanonUpdate.OnClick.AddListener(() => BuyUpgrade(blueTeam, RessourceType.Cannon, blueCannonPrice));
        BluePirateUpdate.OnClick.AddListener(() => BuyUpgrade(blueTeam, RessourceType.Pirate, bluePiratePrice));
        BlueBarrelUpdate.OnClick.AddListener(() => BuyUpgrade(blueTeam, RessourceType.Barrel, blueBarrelPrice));
        BlueShipUpdate.OnClick.AddListener(() => BuyUpgrade(blueTeam, RessourceType.Ship, blueShipPrice));
    }


    public void BuyResource(TeamManager team, RessourceType type, int price)
    {
        if (team == null) {
            Debug.LogWarning("Aucune équipe n'est active au marché ! Appuyez sur R ou B.");
            return;
        }

        if (team.gold >= price)
        {
            team.ModifyResource(RessourceType.Gold, -price);

            if( type == RessourceType.Wood)
            {
                woodNumber = Mathf.Max(0, woodNumber - 1);
            }
            else if(type == RessourceType.Rock)
            {
                rockNumber = Mathf.Max(0, rockNumber - 1);
            }
            else if(type == RessourceType.Chicken)
            {
                chickenNumber = Mathf.Max(0, chickenNumber - 1);
            }
            UpdateStockUI();
            team.ModifyResource(type, 1);
            
            team.AnimateResource(type);
            // --------------------------------

            Debug.Log(team.team + " bought " + type);
        }
        else
        {
            Debug.Log(team.team + " : Not enough Gold!");
        }
    }

    public void SellResource(TeamManager team, RessourceType type, int price)
    {
        if (team == null) {
            Debug.LogWarning("Aucune équipe n'est active au marché ! Appuyez sur R ou B.");
            return;
        }

        bool hasResource = false;
        switch (type)
        {
            case RessourceType.Wood: hasResource = team.wood > 0; break;
            case RessourceType.Rock: hasResource = team.rock > 0; break;
            case RessourceType.Chicken: hasResource = team.chicken > 0; break;
        }

        if (hasResource)
        {
            int sellPrice = Mathf.CeilToInt(price * 0.75f); 
            team.ModifyResource(type, -1);
            team.ModifyResource(RessourceType.Gold, sellPrice);
            if( type == RessourceType.Wood)
            {
                woodNumber += 1;
            }
            else if(type == RessourceType.Rock)
            {
                rockNumber += 1;
            }
            else if(type == RessourceType.Chicken)
            {
                chickenNumber += 1;
            }

            team.AnimateResource(type);
            UpdateStockUI();


            Debug.Log(team.team + " sold " + type);
        }
        else
        {
            Debug.Log(team.team + " : Not enough resources to sell!");
        }
    }

    public void BuyUpgrade(TeamManager team, RessourceType type, int price)
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
            case RessourceType.Cannon: currentLevel = team.cannonLevel; break;
            case RessourceType.Pirate: currentLevel = team.pirateLevel; break;
            case RessourceType.Barrel: currentLevel = team.barrelLevel; break;
            case RessourceType.Ship:   currentLevel = team.shipLevel;   break;
            default: return;
        }

        if (currentLevel >= 5)
        {
            Debug.Log(team.team + " : " + type + " est déjà au niveau Max (5) !");
            return;
        }

        if (team.gold >= price)
        {
            team.ModifyResource(RessourceType.Gold, -price);
            team.ModifyResource(type, 1);
            Debug.Log(team.team + " upgraded " + type + " to level " + (currentLevel + 1));
        }
        else
        {
            Debug.Log(team.team + " : Not enough Gold for upgrade!");
        }
    }

    public void SetText(string text, params Text[] textComponents)
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

        // Liste des types disponibles
        List<RessourceType> availableTypes = new List<RessourceType> { RessourceType.Wood, RessourceType.Rock, RessourceType.Chicken };

        if (modifyTwoResources)
        {
            int index1 = Random.Range(0, availableTypes.Count);
            RessourceType type1 = availableTypes[index1];
            availableTypes.RemoveAt(index1); // On retire pour ne pas la repiocher

            int index2 = Random.Range(0, availableTypes.Count);
            RessourceType type2 = availableTypes[index2];

            // 2. On décide qui monte et qui descend
            bool firstIsUp = (Random.value > 0.5f);
            float mult1 = firstIsUp ? Random.Range(1.2f, 2.5f) : Random.Range(0.4f, 0.8f);
            float mult2 = firstIsUp ? Random.Range(0.4f, 0.8f) : Random.Range(1.2f, 2.5f);
            ApplyPriceModification(type1, mult1);
            ApplyPriceModification(type2, mult2);
        }
        else
        {           
            int index = Random.Range(0, availableTypes.Count);
            RessourceType type = availableTypes[index];
            float randomMult = Random.Range(0.5f, 2.5f);
            
            ApplyPriceModification(type, randomMult);
        }
        UpdatePriceUI();
    }

    void ApplyPriceModification(RessourceType type, float multiplier)
    {
        switch (type)
        {
            case RessourceType.Wood:
                woodPrice = Mathf.CeilToInt(baseWoodPrice * multiplier);
                Debug.Log($"BOIS : {baseWoodPrice} -> {woodPrice} (x{multiplier:F2})");
                int sellWoodPrice = Mathf.CeilToInt(woodPrice * 0.8f);
                UpdateTextColor(woodPrice, baseWoodPrice, buyWoodPriceText);
                UpdateTextColor(sellWoodPrice, baseWoodPrice, sellWoodPriceText);
                break;

            case RessourceType.Rock:
                rockPrice = Mathf.CeilToInt(baseRockPrice * multiplier);
                Debug.Log($"PIERRE : {baseRockPrice} -> {rockPrice} (x{multiplier:F2})");
                int sellRockPrice = Mathf.CeilToInt(rockPrice * 0.8f);
                UpdateTextColor(rockPrice, baseRockPrice, buyRockPriceText);
                UpdateTextColor(sellRockPrice, baseRockPrice, sellRockPriceText);
                break;

            case RessourceType.Chicken:
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

    public void OnMessage(PartyMessage msg)
    {
        // Start fight
        if (msg.message.StartsWith("ask_fight;"))
        {
            if (!stats.GetValue().IsInFight)
            {
                stats.SetValue(new GameStats{
                    IsInFight = true,
                    Winner = null,
                });
                SetActiveTeam(null);
            }
        }

        // End fight
        else if (msg.message.StartsWith("ask_fight_end"))
        {
            if (stats.GetValue().IsInFight)
            {
                stats.SetValue(new GameStats{
                    IsInFight = false,
                    Winner = null,
                });
            }
        }

        // On Open Shop
        else if (msg.message.StartsWith("ask_shop;"))
        {
            var team = Team.Parse(msg.message.Substring("ask_shop;".Length));
            if (activeTeam == null)
            {
                SetActiveTeam(team);
            }
        }

        // On Close Shop
        else if (msg.message.StartsWith("ask_shop_end;"))
        {
            var team = Team.Parse(msg.message.Substring("ask_shop_end;".Length));
            if (activeTeam.team == team)
            {
                SetActiveTeam(null);
            }
        }
    }
}