using UnityEngine;

public class MultiDisplayManager : MonoBehaviour
{
    void Start()
    {
        // Affiche le 2ème écran si disponible
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
        }
    }
}