

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


public class MarketBehaviour : MonoBehaviour
{
    public InventoryServer Inventory;
    public Market MarketGUI;
    public List<TeamServer> Teams;
    public UnityEvent<RessourceType> OnBuy;
    

    public float BuyPriceRation = 0.8f;
    
    [Header("Economic System")]
    public float MinimumChangeInterval = 20f;
    public float MaximumChangeInterval = 60f;
    public int MinimumPrice = 5;
    public int MaximumPrice = 20;
    public int RessourceMultiplier;
    public int UpgradeMultiplier;
    public PriceData Prices;

    void Start()
    {
        foreach(var type in Enum.GetValues(typeof(RessourceType)))
        {
            SetPrice((RessourceType)type, 10);
        }

        StartCoroutine(PriceChangeLoop());
    }

    public int GetPrice(RessourceType type)
    {
        return Prices.Get(type);
    }

    public int GetSellPrice(RessourceType type)
    {
        return (int)(GetPrice(type) * BuyPriceRation);
    }

    public void SetPrice(RessourceType type, int price)
    {
        int sellPrice = (int)(price*BuyPriceRation);

        var counter = MarketGUI.Get(type);
        if (counter != null)
        {
            counter.BuyPrice.SetPrice(price);
            counter.SellPrice.SetPrice(sellPrice);
        }

        foreach(var team in Teams)
        {
            var upgrade = team.TeamGUI.GetUpgradeCounter(type);
            if (upgrade != null)
            {
                upgrade.Price.SetPrice(price);
            }
        }
    }

    IEnumerator PriceChangeLoop()
    {
        while (true)
        {
            // Wait
            var interval = MinimumChangeInterval + (MaximumChangeInterval - MinimumChangeInterval) * UnityEngine.Random.value;
            yield return new WaitForSeconds(interval);

            // Change prices
            foreach(RessourceType type in Enum.GetValues(typeof(RessourceType)))
            {
                var newPrice = MinimumPrice + (MaximumPrice - MinimumPrice) * UnityEngine.Random.value;

                if (RessourceTypes.RESSOURCES.Contains(type))
                {
                    newPrice *= RessourceMultiplier;
                }
                else if (RessourceTypes.UPGRADES.Contains(type))
                {
                    newPrice *= UpgradeMultiplier;
                }

                SetPrice(type, Math.Max(1,(int)newPrice));
            }
        }
    }

    void SetupButton()
    {
        // Buy upgrade
        foreach(var team in Teams)
        {
            foreach(RessourceType type in RessourceTypes.UPGRADES)
            {
                var counter = team.TeamGUI.GetUpgradeCounter(type);
                counter.Button.onClick.AddListener(() =>{
                    if (Inventory.TeamOnShop?.enumValue != team.TeamId)return;

                    var price = GetPrice(type);
                    var values = team.server.GetValue();

                    if(values.gold < price) return;
                    if(values.Get(type)>=4) return;

                    team.SetResource(RessourceType.Gold, values.gold - price);
                    team.SetResource(type, values.Get(type) + 1);
                });
            }
        }

        // Buy ressources
        foreach(RessourceType type in RessourceTypes.RESSOURCES)
        {
            var item = MarketGUI.Get(type);
            item.BuyButton.onClick.AddListener(() =>{
                if (Inventory.TeamOnShop == null) return;

                var team = Teams.First(t => t.TeamId == Inventory.TeamOnShop.enumValue);

                var price = GetPrice(type);
                var values = team.server.GetValue();

                if(values.gold < price) return;

                team.SetResource(RessourceType.Gold, values.gold - price);
                team.SetResource(type, values.Get(type) + 1);
            });

            item.SellButton.onClick.AddListener(() =>{
                if (Inventory.TeamOnShop == null) return;

                var team = Teams.First(t => t.TeamId == Inventory.TeamOnShop.enumValue);

                var price = GetSellPrice(type);
                var values = team.server.GetValue();

                if(values.Get(type) <= 0) return;

                team.SetResource(RessourceType.Gold, values.gold + price);
                team.SetResource(type, values.Get(type) - 1);
            });
        }
    }
}