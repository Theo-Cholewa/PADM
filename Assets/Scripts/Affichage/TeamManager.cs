using UnityEngine;
using UnityEngine.UI; 

public class TeamManager : MonoBehaviour
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

    void Start()
    {
        UpdateUI();
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
        // On vérifie si le texte est assigné avant de le modifier pour éviter les erreurs
        if(goldText) goldText.text = gold.ToString() + "g";
        if(woodText) woodText.text = "x" + wood.ToString();
        if(rockText) rockText.text = "x" + rock.ToString();
        if(chickenText) chickenText.text = "x" + chicken.ToString();

        // Mise à jour des niveaux (si tu les as assignés)
        if(cannonLevelText) cannonLevelText.text = "lvl " + cannonLevel.ToString();
        if(pirateLevelText) pirateLevelText.text = "lvl " + pirateLevel.ToString();
        if(barrelLevelText) barrelLevelText.text = "lvl " + barrelLevel.ToString();
        if(shipLevelText) shipLevelText.text = "lvl " + shipLevel.ToString();
    }
}