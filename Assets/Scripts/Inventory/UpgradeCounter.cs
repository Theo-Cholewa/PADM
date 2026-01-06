using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UpgradeCounter : MonoBehaviour
{
    public Jauge Jauge;
    public UnityEngine.UI.Image GreenButton;
    public UnityEngine.UI.Image GreyButton;
    public Button Button;
    public UnityEvent onPress;
    public PriceTag Price;

    public void SetUpgradable(bool isEnabled)
    {
        GreenButton.gameObject.SetActive(isEnabled);
        GreyButton.gameObject.SetActive(!isEnabled);
    }

    public void SetLevel(int level)
    {
        Jauge.Value = Math.Clamp(level,0,4)/4f;
    }

    void Start()
    {
        Button.onClick.AddListener(()=>{
            if(GreenButton.gameObject.activeSelf) onPress?.Invoke();
        });

        SetUpgradable(false);
        SetLevel(0);
    }
    
}
