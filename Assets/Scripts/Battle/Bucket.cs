using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[RequireComponent(typeof(Physic))]
public class Bucket : MonoBehaviour
{
    public GameObject Water;

    public GameObject WaterPrefab;

    private bool isFilled;

    private Vector3 currentMovement = Vector3.zero;

    private Physic physic;

    void Start()
    {
        SetFilled(true);
        physic = GetComponent<Physic>();
    }

    void SetFilled(bool filled)
    {
        isFilled = filled;
        Water.SetActive(filled);
    }

    Vector3 GetMovementDirection()
    {
        var movementDirection = currentMovement;
        movementDirection.z = 0;
        movementDirection.Normalize();
        return movementDirection;
    }

    float GetMovementSpeed()
    {
        return currentMovement.magnitude;
    }

    Vector3 GetRotationDirection()
    {
        var rotationDirection = transform.TransformPoint(Vector3.up) - transform.position;
        rotationDirection.z = 0;
        rotationDirection.Normalize();
        return rotationDirection;
    }

    float GetFacingCorrelation()
    {
        return Vector3.Dot(GetMovementDirection(), GetRotationDirection());
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check everything
        if (isFilled) return;
        if (collision.gameObject == null) return;
        if (!Tagged.Is(collision.gameObject, "flaque")) return;

        // Check if the bucket is moving sufficiently quickly
        var speed = GetMovementSpeed();
        if (speed < 1f) return;

        // Check if the bucket is moving in the direction his hole is facing.
        var correlation = GetFacingCorrelation();
        if (correlation < 0.8f) return;

        SetFilled(true);
        Destroy(collision.gameObject);
    }

    void FixedUpdate()
    {
        // Est ce que le sceau bouge dans la direction opposée à la direciton à laquelle il fait face.
        if (isFilled)
        {
            if (GetMovementSpeed() > 1f)
            {
                var correlation = GetFacingCorrelation();
                if (correlation < -0.8f)
                {
                    SetFilled(false);
                    var created = Instantiate(WaterPrefab, transform);
                    created.transform.localPosition = new Vector3(0,0,0.01f);
                    created.transform.parent = null;
                    created.transform.localScale = new Vector3(3, 3, 3);
                    created.transform.localRotation = Quaternion.identity;
                }
            }
        }

        currentMovement = physic.velocity * 0.2f + currentMovement * 0.8f;
    }

    void Update()
    {
        Debug.DrawLine(transform.position, transform.position + currentMovement*10, Color.red);
    }
}
