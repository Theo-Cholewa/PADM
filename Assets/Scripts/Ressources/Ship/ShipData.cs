using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShipData : MonoBehaviour
{
    [Header("Couleur du bateau")]
    public Color shipColor = Color.red;

    [Header("Statistiques principales")]
    public int life = 100;
    public int sailors = 3;

    [Header("Ressources à bord")]
    public int food = 0;
    public int wood = 0;
    public int stone = 0;
    public int gunpowder = 0;
    public int cannonballs = 0; 

    [Header("Améliorations du bateau")]
    public List<string> upgrades = new List<string>();

    // 🔹 Événement déclenché quand les ressources changent
    public Action OnResourcesChanged;

    // --- Méthodes utilitaires ---

    public void AddResource(string type, int amount)
    {
        switch (type.ToLower())
        {
            case "food":       food += amount; break;
            case "wood":       wood += amount; break;
            case "stone":      stone += amount; break;
            case "gunpowder":  gunpowder += amount; break;
            case "cannonballs": cannonballs += amount; break;
            default:
                Debug.LogWarning($"Type de ressource inconnu : {type}");
                return;
        }

        ClampResources();
        OnResourcesChanged?.Invoke(); // 🔔 avertit le contrôleur
    }

    public void RemoveResource(string type, int amount)
    {
        switch (type.ToLower())
        {
            case "food":       food = Mathf.Max(0, food - amount); break;
            case "wood":       wood = Mathf.Max(0, wood - amount); break;
            case "stone":      stone = Mathf.Max(0, stone - amount); break;
            case "gunpowder":  gunpowder = Mathf.Max(0, gunpowder - amount); break;
            case "cannonballs": cannonballs = Mathf.Max(0, cannonballs - amount); break;
            default:
                Debug.LogWarning($"Type de ressource inconnu : {type}");
                return;
        }

        ClampResources();
        OnResourcesChanged?.Invoke(); // 🔔 avertit le contrôleur
    }

    public void TakeDamage(int amount)
    {
        life = Mathf.Max(0, life - amount);
    }

    public void Heal(int amount)
    {
        life = Mathf.Min(100, life + amount);
    }

    public void AddUpgrade(string upgradeName)
    {
        if (!upgrades.Contains(upgradeName))
            upgrades.Add(upgradeName);
    }

    // 🔹 Clamp pour éviter de dépasser 10 par exemple
    private void ClampResources()
    {
        food        = Mathf.Clamp(food, 0, 10);
        wood        = Mathf.Clamp(wood, 0, 10);
        stone       = Mathf.Clamp(stone, 0, 10);
        gunpowder   = Mathf.Clamp(gunpowder, 0, 10);
        cannonballs = Mathf.Clamp(cannonballs, 0, 10);
    }
}