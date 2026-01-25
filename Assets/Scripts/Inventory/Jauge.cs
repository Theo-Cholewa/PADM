using System;
using UnityEngine;

public class Jauge : MonoBehaviour
{
    public UnityEngine.UI.Image Filler;
    
    public float Value
    {
        set => Filler.rectTransform.anchorMax = new Vector2(Math.Max(0, Math.Min(1, value)), 1);
    }
}
