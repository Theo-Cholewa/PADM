using UnityEngine;
using UnityEngine.UI;

public class PriceTag : MonoBehaviour
{
    public UnityEngine.UI.Image Icon;
    public Text Text;
    public Text BackText;

    public void SetPrice(int value)
    {
        Text.text = $"{value}";
        BackText.text = $"{value}";
    }
}
