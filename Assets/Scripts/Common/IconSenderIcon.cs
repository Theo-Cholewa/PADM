using UnityEngine;

public class IconSenderIcon : MonoBehaviour
{
    public static float ICON_MOVE_SPEED = 0.02f;

    public GameObject Display;

    [HideInInspector] public IconSender sender;
    [HideInInspector] public Vector2 worldPosition;
    [HideInInspector] public Vector2 targetPosition;

    void Start()
    {
        var IconPos = Vector2Int.FloorToInt(worldPosition);
        var localPosition = worldPosition-IconPos;
        transform.localPosition = (localPosition-new Vector2(.5f,.5f))*sender.Zone.rect.size;
    }

    void Update()
    {
        // Move
        var delta = targetPosition - worldPosition;

        // Kill
        if (delta.sqrMagnitude < ICON_MOVE_SPEED)
        {
            Destroy(gameObject);
            return;
        }

        // Move
        delta.Normalize();
        delta *= ICON_MOVE_SPEED;
        worldPosition += delta;

        // Change visual position
        var MyPosition = sender.placement.Position;
        var IconPos = Vector2Int.FloorToInt(worldPosition);

        // Move on screen
        if (MyPosition==IconPos)
        {
            var localPosition = worldPosition-IconPos;
            transform.localPosition = (localPosition-new Vector2(.5f,.5f))*sender.Zone.rect.size;
            Display.SetActive(true);
        }
        else Display.SetActive(false);
    }
}