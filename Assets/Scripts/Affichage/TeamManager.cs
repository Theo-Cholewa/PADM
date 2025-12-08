using UnityEngine;
using UnityEngine.UI;

public partial class TeamManager : MonoBehaviour
{
    [Header("Identité")]
    public string teamName = "Red"; 

    [Header("Ressources Actuelles")]
    public int gold = 150; 
    public int wood = 0;
    public int rock = 0;
    public int chicken = 0;

    [Header("Niveaux Upgrades")]
    public int cannonLevel = 1;
    public int pirateLevel = 1;
    public int barrelLevel = 1;
    public int shipLevel = 1;

    [Header("Références UI (Glisser les Legacy Text ici)")]
    public Text goldText;
    public Text woodText;
    public Text rockText;
    public Text chickenText;
    
    [Header("Références UI Upgrades (Optionnel)")]
    public Text cannonLevelText;
    public Text pirateLevelText;
    public Text barrelLevelText;
    public Text shipLevelText;

    [Header("Références UI (Jauges / Barres)")]
    public Image cannonBarImage;
    public Image pirateBarImage;
    public Image barrelBarImage;
    public Image shipBarImage;
    private const float MAX_LEVEL = 5f;

    void Start()
    {
        UpdateUI();
        sharedDataServer = new PartyTools.ValueServer<StoreData>(
            Party.current,
            $"team_{teamName.ToLower()}",
            new StoreData
            {
                gold = gold,
                wood = wood,
                rock = rock,
                chicken = chicken,
                cannonLevel = cannonLevel,
                pirateLevel = pirateLevel,
                barrelLevel = barrelLevel,
                shipLevel = shipLevel
            },
            (sharedData) => JsonUtility.ToJson(sharedData)
        );
        Party.current.OnMessage.AddListener(OnMessage);
    }

    void OnDestroy()
    {
        Party.current.OnMessage.RemoveListener(OnMessage);
    }

    public void ModifyResource(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Wood: wood += amount; break;
            case ResourceType.Rock: rock += amount; break;
            case ResourceType.Chicken: chicken += amount; break;
            
            case ResourceType.Cannon: cannonLevel += amount; break;
            case ResourceType.Pirate: pirateLevel += amount; break;
            case ResourceType.Barrel: barrelLevel += amount; break;
            case ResourceType.Ship: shipLevel += amount; break;
        }
        UpdateNetwork();
        UpdateUI();
    }

    public void ModifyGold(int amount)
    {
        gold += amount;
        UpdateUI();
    }

    // Met à jour l'affichage
    public void UpdateUI()
    {
        if(goldText) goldText.text = gold.ToString() + "g";
        if(woodText) woodText.text = "x" + wood.ToString();
        if(rockText) rockText.text = "x" + rock.ToString();
        if(chickenText) chickenText.text = "x" + chicken.ToString();

        // Mise à jour des niveaux (si tu les as assignés)
        if(cannonLevelText) cannonLevelText.text = "lvl" + cannonLevel.ToString();
        if(pirateLevelText) pirateLevelText.text = "lvl" + pirateLevel.ToString();
        if(barrelLevelText) barrelLevelText.text = "lvl" + barrelLevel.ToString();
        if(shipLevelText) shipLevelText.text = "lvl" + shipLevel.ToString();

        float steps = MAX_LEVEL - 1;

        if (cannonBarImage) cannonBarImage.fillAmount = (cannonLevel - 1) / steps;
        if (pirateBarImage) pirateBarImage.fillAmount = (pirateLevel - 1) / steps;
        if (barrelBarImage) barrelBarImage.fillAmount = (barrelLevel - 1) / steps;
        if (shipBarImage)   shipBarImage.fillAmount = (shipLevel - 1)   / steps;
    }

    PartyTools.ValueServer<StoreData> sharedDataServer;

    void UpdateNetwork()
    {
        sharedDataServer.SetValue(new StoreData
        {
            gold = gold,
            wood = wood,
            rock = rock,
            chicken = chicken,
            cannonLevel = cannonLevel,
            pirateLevel = pirateLevel,
            barrelLevel = barrelLevel,
            shipLevel = shipLevel
        });
    }

    void OnMessage(PartyMessage message)
    {
        if (message.message.StartsWith("store;add;"))
        {
            var param = message.message.Split(';');
            if(param[2]!=teamName.ToLower())return;
            var value = int.Parse(param[3]);
            var type = param[4];
            if(type=="gold") gold += value;
            else if(type=="wood") wood += value;
            else if(type=="rock") rock += value;
            else if(type=="chicken") chicken += value;
            UpdateUI();
            UpdateNetwork();
        }
    }
}