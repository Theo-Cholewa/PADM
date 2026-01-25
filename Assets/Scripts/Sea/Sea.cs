using UnityEngine;

public class Sea : MonoBehaviour
{
    public Transform Rotatif;
    public MeshRenderer Colored;

    public static Team Team = null;

    private float start;

    void Start()
    {
        var color = Team?.color ?? Color.gray;
        Colored.material.color = color;
        start = Time.time;
    }

    void Update()
    {
        Rotatif.rotation = Quaternion.Euler(0, 0, -30f*(Time.time-start));
    }
}
