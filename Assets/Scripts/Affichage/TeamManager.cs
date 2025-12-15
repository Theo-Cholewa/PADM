using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public partial class TeamManager : MonoBehaviour
{
    [Header("Identité")]
    public Team team = Team.RED; 

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

            case ResourceType.Gold: gold += amount; break;
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
    }

    // --- AJOUTER CECI À LA FIN DE TEAMMANAGER.CS ---

    public void AnimateResource(ResourceType type)
    {
        RawImage targetImage = null;

        switch (type)
        {
            case ResourceType.Wood:
                targetImage = woodImage;
                break;
            case ResourceType.Rock:
                targetImage = rockImage;
                break;
            case ResourceType.Chicken:
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

            // Formule mathématique (Sinus) pour faire un aller-retour fluide : 0 -> 1 -> 0
            // Cela permet de grossir puis rétrécir
            float scaleAmount = Mathf.Sin(progress * Mathf.PI); 
            
            // On applique le scale : Base + (Extra * courbe)
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
            shipLevel = shipLevel
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
            var type = Enum.Parse<ResourceType>(typeName);
            ModifyResource(type, value);
            UpdateUI();
            UpdateNetwork();
        }
    }
}