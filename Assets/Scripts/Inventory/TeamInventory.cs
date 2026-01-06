using System;
using UnityEngine;
using UnityEngine.UI;

public class TeamInventory : MonoBehaviour
{
    [Header("Resources")]
    public GameObject PopupPrefab;
    public GameObject PopdownPrefab;


    [Header("Content")]
    public RessourceCounter Stone;
    public RessourceCounter Wood;
    public RessourceCounter Chicken;

    public UpgradeCounter Canon;
    public UpgradeCounter Pirate;
    public UpgradeCounter Barrel;
    public UpgradeCounter Ship;

    public Text Gold;
    public Jauge Health;


    private int currentHealth=100;
    private int currentMoney=0;


    void Start()
    {
        Health.Value = 1;
        Gold.text = "0";
    }

    public void SetHealth(int value)
    {
        var newValue = Math.Clamp(value, 0, 100);
        var offset = value-currentHealth;

        currentHealth = newValue;
        Health.Value = currentHealth / 100f;

        Popup.Spawn(Health.transform, PopupPrefab, PopdownPrefab, offset);
    }

    public void SetMoney(int value)
    {
        var newValue = Math.Clamp(value, 0, 100);
        var offset = value-currentMoney;

        currentMoney = newValue;
        Gold.text = currentMoney.ToString();

        Popup.Spawn(Gold.transform, PopupPrefab, PopdownPrefab, offset);
    }

    public void SetRessource(RessourceType type, int value)
    {
        switch(type)
        {
            case RessourceType.Stone: Stone.SetCount(value); break;
            case RessourceType.Wood: Wood.SetCount(value); break;
            case RessourceType.Chicken: Chicken.SetCount(value); break;

            case RessourceType.Cannon: Canon.SetLevel(value); break;
            case RessourceType.Pirate: Pirate.SetLevel(value); break;
            case RessourceType.Barrel: Barrel.SetLevel(value); break;
            case RessourceType.Ship: Ship.SetLevel(value); break;

            case RessourceType.Health: SetHealth(value); break;
        }
    }

    public RessourceCounter GetRessourceCounter(RessourceType type)
    {
        switch(type)
        {
            case RessourceType.Stone: return Stone;
            case RessourceType.Wood: return Wood;
            case RessourceType.Chicken: return Chicken;
            default: return null;
        }
    }

    public UpgradeCounter GetUpgradeCounter(RessourceType type)
    {
        switch(type)
        {
            case RessourceType.Cannon: return Canon;
            case RessourceType.Pirate: return Pirate;
            case RessourceType.Barrel: return Barrel;
            case RessourceType.Ship: return Ship;
            default: return null;
        }
    }

    public void SetUpgradable(bool upgradable)
    {
        Canon.SetUpgradable(upgradable);
        Pirate.SetUpgradable(upgradable);
        Barrel.SetUpgradable(upgradable);
        Ship.SetUpgradable(upgradable);
    }
}
