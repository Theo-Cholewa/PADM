using UnityEngine;

public class VolantButton : MonoBehaviour
{

    public Volant Volant;
    public MeshRenderer Colored;


    void OnTouchDown(TouchInfo info)
    {
        if (Volant.speed > .5f)
        {
            Volant.speed = 0f;
            Colored.material.color = Color.white;
        }
        else
        {
            Volant.speed = 1f;
            Colored.material.color = Color.green;
        }
    }
}
