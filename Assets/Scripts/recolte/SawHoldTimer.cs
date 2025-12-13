using UnityEngine;
using UnityEngine.EventSystems;

public class SawHoldTimer : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Références")]
    public WoodHarvestController woodHarvest;
    public TouchIndicatorWaveMulti[] holdIndicators; // les 2 triangles

    [Header("Timing")]
    public float requiredHoldSeconds = 3f;

    private bool pointerDown = false;
    private float timer = 0f;
    private bool finished = false;

    void Update()
    {
        if (finished) return;
        if (!pointerDown)
        {
            timer = 0f;
            return;
        }

        // Prérequis : les 2 hold-touch-indicator doivent être validés
        if (!AreAllTouched(holdIndicators))
        {
            timer = 0f;
            return;
        }

        timer += Time.deltaTime;

        if (timer >= requiredHoldSeconds)
        {
            finished = true;
            woodHarvest.FinishHarvest();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDown = true;
        timer = 0f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerDown = false;
        timer = 0f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerDown = false;
        timer = 0f;
    }

    private bool AreAllTouched(TouchIndicatorWaveMulti[] list)
    {
        if (list == null || list.Length == 0) return false;
        foreach (var t in list)
        {
            if (t == null || !t.isTouched)
                return false;
        }
        return true;
    }
}
