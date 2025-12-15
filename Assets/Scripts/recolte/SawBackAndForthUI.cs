using UnityEngine;

public class SawBackAndForthUI : MonoBehaviour
{
    public RectTransform handA;
    public RectTransform handB;

    [Header("Mouvement")]
    public float amplitude = 60f;
    public float baseSpeed = 2f;
    public float boostedSpeed = 4.5f;

    private float currentSpeed;
    private Vector3 handAStartPos;
    private Vector3 handBStartPos;

    void Start()
    {
        handAStartPos = handA.localPosition;
        handBStartPos = handB.localPosition;
        currentSpeed = baseSpeed;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * currentSpeed) * amplitude;
        Vector3 movement = new Vector3(0f, 0f, offset);

        handA.localPosition = handAStartPos + movement;
        handB.localPosition = handBStartPos + movement;
    }

    public void SetBoosted(bool boosted)
    {
        currentSpeed = boosted ? boostedSpeed : baseSpeed;
    }
}
