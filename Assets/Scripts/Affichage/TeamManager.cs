using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public partial class TeamManager : MonoBehaviour
{
    [Header("Identité")]
    public TeamEnum TeamId; 

    public Team team => Team.Of(TeamId);

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
    [Header("Img ressources ")]
    public RawImage woodImage;
    public RawImage rockImage;
    public RawImage chickenImage;

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
            $"team_{Team.Of(TeamId).id}",
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


    public void AnimateResource(RessourceType type)
    {
        RawImage targetImage = null;

        switch (type)
        {
            case RessourceType.Wood:
                targetImage = woodImage;
                break;
            case RessourceType.Rock:
                targetImage = rockImage;
                break;
            case RessourceType.Chicken:
                targetImage = chickenImage;
                break;
        }

        if (targetImage != null)
        {
            StopAllCoroutines(); 
            StartCoroutine(PulseImage(targetImage.rectTransform, 0.5f, 1.5f));
        }
    }

    IEnumerator PulseImage(RectTransform target, float duration, float maxScale)
    {
        Vector3 originalScale = Vector3.one; // On part du principe que la taille de base est 1,1,1
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration; // De 0 à 1

            float scaleAmount = Mathf.Sin(progress * Mathf.PI); 
            
            float currentScale = 1f + ( (maxScale - 1f) * scaleAmount );

            target.localScale = originalScale * currentScale;

            yield return null;
        }

        // Sécurité : on remet l'échelle exacte à la fin
        target.localScale = originalScale;
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
            if(param[2]!=Team.Of(TeamId).id)return;
            var value = int.Parse(param[3]);
            var typeName = param[4];
            var type = Enum.Parse<RessourceType>(typeName);
            ModifyResource(type, value);
            UpdateUI();
            UpdateNetwork();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ModifyResource(RessourceType.Wood,10);
            ModifyResource(RessourceType.Rock,5);
            ModifyResource(RessourceType.Chicken,3);
        }
    }
}