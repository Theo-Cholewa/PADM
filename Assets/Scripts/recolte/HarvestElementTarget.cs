using UnityEngine;
using UnityEngine.EventSystems;

public class HarvestElementTarget : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector] public bool clicked = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        clicked = true;
        // Debug.Log($"✅ Click enregistré sur {gameObject.name}");
    }

    public void ResetClick()
    {
        clicked = false;
    }
}
