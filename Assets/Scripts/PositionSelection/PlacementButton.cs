using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlacementButton : MonoBehaviour
{
    public PositionSelection Menu;
    public Button Button;
    public Text Text;

    public Vector2Int Position;

    void Start()
    {
        Menu.onChange.AddListener(OnRoleChange);
        OnRoleChange();
    }

    void OnDestroy()
    {
        Menu.onChange.RemoveListener(OnRoleChange);
    }

    void OnValidate()
    {
        Text.text = $"{Position.x} {Position.y}";
        gameObject.name = Text.text;
    }

    void OnRoleChange()
    {
        Button.interactable = !Menu.UsedRoles.GetValues().Any(it=>it.Value==Position);
    }
    
}
