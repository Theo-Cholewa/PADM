using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SawBackAndForthZ : MonoBehaviour
{
    [Header("Motion")]
    [Tooltip("Amplitude du mouvement sur Z (unités locales)")]
    public float amplitude = 30f;

    [Tooltip("Vitesse du va-et-vient")]
    public float speed = 3f;

    [Tooltip("Inverse le sens si besoin")]
    public bool invert = false;

    private RectTransform rt;
    private Vector3 startLocalPos;
    private bool initialized;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        // Capture la position exacte au moment où l'objet apparaît (phase 2)
        startLocalPos = rt.localPosition;
        initialized = true;
    }

    private void OnDisable()
    {
        if (rt != null) rt.localPosition = startLocalPos;
        initialized = false;
    }

    private void Update()
    {
        if (!initialized) return;

        float t = Mathf.Sin(Time.time * speed); // -1..+1
        if (invert) t = -t;

        // ✅ déplacement uniquement sur Z
        rt.localPosition = new Vector3(
            startLocalPos.x,
            startLocalPos.y,
            startLocalPos.z + t * amplitude
        );
    }
}
