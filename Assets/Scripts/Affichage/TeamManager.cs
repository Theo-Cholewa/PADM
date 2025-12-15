using System;
using UnityEngine;
using UnityEngine.UI;

public partial class TeamManager : MonoBehaviour
{
    [Header("Identité")]
    public Team team = Team.RED; 

    public GameObject Popup;
    public GameObject Popdown;

    public int health = 100;

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

    [Header("Références UI (Jauges / Barres)")]
    public Image cannonBarImage;
    public Image pirateBarImage;
    public Image barrelBarImage;
    public Image shipBarImage;
    public Image healthBarImage;
    private const float MAX_LEVEL = 5f;

    void Start()
    {
        UpdateUI();

        if (Party.current == null)
        {
            Debug.LogError("TeamManager: Party.current is NULL! Make sure a Party object is in the scene and initialized.");
            return;
        }

        sharedDataServer = new PartyTools.ValueServer<RessourceData>(
            Party.current,
            $"team_{team.id}",
            new RessourceData
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

    public void ModifyResource(RessourceType type, int amount)
    {
        // Animation
        GameObject icon = null;
        switch (type)
        {
            case RessourceType.Wood: icon = woodText.gameObject; break;
            case RessourceType.Rock: icon = rockText.gameObject; break;
            case RessourceType.Chicken: icon = chickenText.gameObject; break;

            case RessourceType.Cannon: icon = cannonBarImage.gameObject; break;
            case RessourceType.Pirate: icon = pirateBarImage.gameObject; break;
            case RessourceType.Barrel: icon = barrelBarImage.gameObject; break;
            case RessourceType.Ship: icon = shipBarImage.gameObject; break;

            case RessourceType.Gold: icon = goldText.gameObject; break;

            case RessourceType.Health: icon = healthBarImage.gameObject; break;
        }
        if (icon != null)
        {
            if (amount > 0)
            {
                var effect = Instantiate(Popup,icon.transform);
                effect.GetComponentInChildren<Text>().text = amount.ToString();
            }
            else if (amount < 0)
            {
                var effect = Instantiate(Popdown,icon.transform);
                effect.GetComponentInChildren<Text>().text = "-"+(-amount).ToString();
            }
        }

        // Change value
        switch (type)
        {
            case RessourceType.Wood: wood += amount; break;
            case RessourceType.Rock: rock += amount; break;
            case RessourceType.Chicken: chicken += amount; break;
            
            case RessourceType.Cannon: cannonLevel += amount; break;
            case RessourceType.Pirate: pirateLevel += amount; break;
            case RessourceType.Barrel: barrelLevel += amount; break;
            case RessourceType.Ship: shipLevel += amount; break;

            case RessourceType.Gold: gold += amount; break;
            case RessourceType.Health: health += amount; break;
        }
        UpdateNetwork();
        UpdateUI();
    }

    public void UpdateUI()
    {
        if(goldText) goldText.text = gold.ToString() + "g";
        if(woodText) woodText.text = "x" + wood.ToString();
        if(rockText) rockText.text = "x" + rock.ToString();
        if(chickenText) chickenText.text = "x" + chicken.ToString();

        float steps = MAX_LEVEL - 1;

        if (cannonBarImage) cannonBarImage.fillAmount = (cannonLevel - 1) / steps;
        if (pirateBarImage) pirateBarImage.fillAmount = (pirateLevel - 1) / steps;
        if (barrelBarImage) barrelBarImage.fillAmount = (barrelLevel - 1) / steps;
        if (shipBarImage)   shipBarImage.fillAmount = (shipLevel - 1)   / steps;
        if (healthBarImage) healthBarImage.fillAmount = health/100f;
    }

    PartyTools.ValueServer<RessourceData> sharedDataServer;

    void UpdateNetwork()
    {
        sharedDataServer.SetValue(new RessourceData
        {
            gold = gold,
            wood = wood,
            rock = rock,
            chicken = chicken,
            cannonLevel = cannonLevel,
            pirateLevel = pirateLevel,
            barrelLevel = barrelLevel,
            shipLevel = shipLevel,
            health = health
        });
    }

    void OnMessage(PartyMessage message)
    {
        if (message.message.StartsWith("store;add;"))
        {
            var param = message.message.Split(';');
            if(param[2]!=team.id)return;
            var value = int.Parse(param[3]);
            var typeName = param[4];
            var type = Enum.Parse<RessourceType>(typeName);
            ModifyResource(type, value);
            UpdateUI();
            UpdateNetwork();
        }
    }
}