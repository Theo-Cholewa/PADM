using UnityEngine;

public class BucketPlane : MonoBehaviour
{
    public GameObject water;
    public Transform hole;  
    public GameObject puddle;
    public float showDistance = 1f;
    public float rotationSpeed = 90f;

    private bool waterVisible = false;
    private bool rotating = false;
    private Quaternion targetRotation;
    private bool rotated = false;

    private Quaternion initialRotation;
    private Renderer puddleRenderer;

    void Start()
    {
        if (water != null)
            water.SetActive(false);

        initialRotation = transform.rotation;

        if (puddle != null)
            puddleRenderer = puddle.GetComponent<Renderer>();
    }

    void Update()
    {
        if (hole == null || water == null) return;

        bool puddleVisible = puddleRenderer != null && puddleRenderer.isVisible;

        if (!waterVisible && puddleVisible)
        {
            float distance = Vector3.Distance(transform.position, hole.position);
            if (distance < showDistance)
            {
                water.SetActive(true);
                waterVisible = true;
                if (puddle != null)
                {
                    puddle.SetActive(false);
                }

                if (hole != null && !hole.gameObject.activeSelf)
                {
                    hole.gameObject.SetActive(true);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Q) && !rotating)
        {
            if (!rotated)
            {
                targetRotation = Quaternion.Euler(179.286f, -89.99799f, -270.064f);
            }
            else
            {
                targetRotation = initialRotation;
            }

            rotating = true;
            rotated = !rotated;
        }

        if (rotating)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.01f)
            {
                transform.rotation = targetRotation;
                rotating = false;
            }
        }

        if (rotated && waterVisible && water != null)
        {
            water.SetActive(false);
            waterVisible = false;
        }

        if (!waterVisible && puddleVisible)
        {
            float distance = Vector3.Distance(transform.position, hole.position);
            if (distance < showDistance)
            {
                water.SetActive(true);
                waterVisible = true;
                Debug.Log("💧 Water collected");

                // faire disparaître la flaque
                if (puddle != null) puddle.SetActive(false);

                // faire réapparaître le hole
                if (hole != null) hole.gameObject.SetActive(true);
            }
        }
    }
}
