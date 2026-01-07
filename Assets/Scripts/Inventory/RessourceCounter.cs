using UnityEngine;
using UnityEngine.UI;

public class RessourceCounter : MonoBehaviour
{
    public GameObject Icon;
    public Text Text;
    public GameObject PopupPrefab;
    public GameObject PodownPrefab;

    private int CurrentCount = 1;

    void Awake()
    {
        CurrentCount = 0;
        Text.text = "x0";
    }

    public void SetCount(int count)
    {
        // Offset
        var offset = count-CurrentCount;
        Popup.Spawn(Text.transform, PopupPrefab, PodownPrefab, offset);
        if(offset!=0) Popup.Pwomp(this, Icon.transform);

        CurrentCount = count;

        // Set text
        Text.text = $"x{count}";

    }
}
