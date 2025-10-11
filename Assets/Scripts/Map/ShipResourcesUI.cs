using UnityEngine;
using UnityEngine.UI;

public class ShipResourcesUI : MonoBehaviour
{
    public Image foodBar;
    public Image woodBar;
    public Image stoneBar;

    [Header("Max Resource Values")]
    public float maxFood = 100f;
    public float maxWood = 100f;
    public float maxStone = 100f;

    public void UpdateResources(float food, float wood, float stone)
    {
        foodBar.fillAmount = Mathf.Clamp01(food / maxFood);
        woodBar.fillAmount = Mathf.Clamp01(wood / maxWood);
        stoneBar.fillAmount = Mathf.Clamp01(stone / maxStone);
    }
}