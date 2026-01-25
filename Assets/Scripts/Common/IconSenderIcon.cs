using System;
using UnityEngine;

public class IconSenderIcon : MonoBehaviour
{
    public static float ICON_MOVE_SPEED = 0.01f;

    public GameObject Display;

    public bool GonnaDie = false;

    [HideInInspector] public IconSender sender;
    [HideInInspector] public Vector2 worldPosition;
    [HideInInspector] public Vector2 targetPosition;

    void Start()
    {
        transform.position = sender.WorldToScene(worldPosition);
    }

    void Update()
    {
        if (GonnaDie)
        {
            Destroy(gameObject);
            return;
        }

        // Move
        var delta = targetPosition - worldPosition;

        // Kill
        if (delta.sqrMagnitude < ICON_MOVE_SPEED)
        {
            
            GonnaDie = true;
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
            transform.position = sender.WorldToScene(worldPosition);
            Display.SetActive(true);
        }
        else Display.SetActive(false);
    }
}