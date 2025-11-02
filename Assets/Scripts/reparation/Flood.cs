using UnityEngine;

public class Flood : MonoBehaviour
{
    public GameObject hole;
    public GameObject puddle;
    public float visibleDuration = 15f;

    private float visibleTimer = 0f;
    private Renderer holeRenderer;

    void Start()
    {
        if (hole != null)
            holeRenderer = hole.GetComponent<Renderer>();

        if (puddle != null)
            puddle.SetActive(false);
    }

    void Update()
    {
        if (hole == null || holeRenderer == null) return;

        if (holeRenderer.isVisible && hole.activeSelf)
        {
            visibleTimer += Time.deltaTime;
        }
        else
        {
            visibleTimer = 0f;
        }

        if (visibleTimer >= visibleDuration)
        {
            TransformHoleToPuddle();
            visibleTimer = 0f;
        }
    }

    void TransformHoleToPuddle()
    {
        if (hole != null)
            hole.SetActive(false);

        if (puddle != null)
        {
            puddle.SetActive(true);
            puddle.transform.position = hole.transform.position;
        }
    }

    public void ReappearHole()
    {
        if (hole != null)
        {
            hole.SetActive(true);
        }
    }
}
