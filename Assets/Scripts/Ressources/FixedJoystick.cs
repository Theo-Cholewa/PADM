using UnityEngine;
using UnityEngine.EventSystems;

public class FixedJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [Header("Références UI")]
    public RectTransform background;
    public RectTransform handle;

    [Header("Paramètres")]
    public float handleLimit = 100f; // rayon max en pixels

    private Vector2 input = Vector2.zero;

    public Vector2 Direction => input; // direction normalisée (x,y)
    public bool IsActive => input.magnitude > 0.1f; // pour détecter si hors position neutre

    private Vector2 startPos;

    void Start()
    {
        if (background == null)
            background = GetComponent<RectTransform>();
        startPos = background.position;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out pos);

        input = pos / handleLimit;
        if (input.magnitude > 1f)
            input = input.normalized;

        handle.anchoredPosition = input * handleLimit;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        input = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}