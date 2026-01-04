using UnityEngine;
using UnityEngine.EventSystems;

public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public bool IsPressed { get; private set; }
    public Vector2 TouchPosition { get; private set; }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsPressed = true;
        TouchPosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsPressed = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        TouchPosition = eventData.position;
    }
}