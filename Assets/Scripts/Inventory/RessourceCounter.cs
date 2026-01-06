using UnityEngine;
using UnityEngine.UI;

public class RessourceCounter : MonoBehaviour
{
    public Text Text;
    public GameObject PopupPrefab;
    public GameObject PodownPrefab;

    private int CurrentCount = 1;

    void Start()
    {
        CurrentCount = 0;
        Text.text = "x0";
    }

    public void SetCount(int count)
    {
        // Offset
        var offset = count-CurrentCount;
        if (offset > 0)
        {
            var effect = Instantiate(PopupPrefab, Text.transform);
            effect.GetComponentInChildren<Text>().text = "+"+offset.ToString();
        }
        else if (offset < 0)
        {
            var effect = Instantiate(PodownPrefab, Text.transform);
            effect.GetComponentInChildren<Text>().text = "-"+(-offset).ToString();
        }

        // Set text
        Text.text = $"x{count}";
    }
}
