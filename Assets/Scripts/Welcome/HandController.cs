using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("Fingers to monitor")]
    public TouchIndicatorWave[] fingers;

    [Header("Global state")]
    public bool activated = false;

    private bool previousState = false;

    void Update()
    {
        if (fingers == null || fingers.Length == 0)
            return;

        bool allActive = true;
        foreach (var finger in fingers)
        {
            if (finger == null || !finger.isTouched)
            {
                allActive = false;
                break;
            }
        }

        if (allActive && !previousState)
        {
            activated = true;
            Debug.Log("🖐️ Tous les doigts sont activés !");
        }

        // Si au moins un doigt se relâche, on remet à false
        if (!allActive && previousState)
        {
            activated = false;
        }

        previousState = allActive;
    }
}
