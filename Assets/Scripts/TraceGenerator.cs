using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TraceGenerator : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float Frequency = 0.5f;
    public int Size = 0;

    private List<Vector3> Points = new();

    void Start()
    {
        StartCoroutine(Loop());
    }

    void Update()
    {
        if (Points.Count > 0)
        {
            Points[Points.Count - 1] = transform.position;
            lineRenderer.SetPosition(Points.Count - 1, transform.position);
        }
        if (Points.Count > 1)
        {
            Points[0] = Points[0]*0.98f + Points[1]*0.02f;
            lineRenderer.SetPosition(0, Points[0]);
        }
    }

    IEnumerator Loop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Frequency);
            Points.Add(transform.position);
            if (Points.Count > Size)
            {
                Points.RemoveAt(0);
            }
            lineRenderer.positionCount = Points.Count;
            lineRenderer.SetPositions(Points.ToArray());
        }
    }

}
