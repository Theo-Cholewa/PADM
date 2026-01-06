using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    public Text StockText;
    public Button BuyButton;
    public Button SellButton;
    public PriceTag BuyPrice;
    public PriceTag SellPrice;


    private int CurrentStock=0;

    public void SetStock(int value)
    {
        StockText.text = $"Stock: {value}";
        BuyButton.gameObject.SetActive(value>0);
        CurrentStock = value;
    }

    void Start()
    {
        SetStock(0);
    }

}
