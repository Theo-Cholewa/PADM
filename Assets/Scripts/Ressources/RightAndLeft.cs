using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightAndLeft : MonoBehaviour
{
    [Tooltip("Distance (units) to move from the start position for each leg")]
    public float distance = 1f;

    [Tooltip("Speed in units per second (how fast it moves)")]
    public float moveSpeed = 1f;

    [Tooltip("If true, pressing Enter toggles movement start/stop. If false movement is automatic.")]
    public bool toggleWithEnter = true;

    // internal state
    private Vector3 startPos;
    private Vector3 targetPos;
    private bool moving = false;
    private bool forward = true; // direction: true = start -> target, false = target -> start
    private float progress = 0f; // 0..1 progress along current leg

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + new Vector3(distance, 0f, 0f);
    }

    void Update()
    
    {
        // toggle movement when pressing Enter (Return)
        if (toggleWithEnter && Input.GetKeyDown(KeyCode.Return))
        {
            moving = !moving;
            // reset progress so movement starts cleanly from current position
            progress = 0f;
            // ensure startPos and targetPos are consistent with current position
            startPos = transform.position;
            targetPos = startPos + (forward ? new Vector3(distance, 0f, 0f) : new Vector3(-distance, 0f, 0f));
        }

        if (!moving && toggleWithEnter) return;

        // continuous movement when not using toggleWithEnter or when toggled on
        // advance progress by time depending on speed and leg distance
        float legDuration = (moveSpeed <= 0f) ? float.MaxValue : Mathf.Abs(distance) / moveSpeed;
        if (legDuration <= 0f) legDuration = 0.0001f;

        if (legDuration != float.MaxValue)
            progress += Time.deltaTime / legDuration;

        // lerp between startPos and targetPos
        transform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, progress));

        if (progress >= 1f)
        {
            // flip direction and prepare next leg
            forward = !forward;
            progress = 0f;
            // ensure exact snapping to target
            transform.position = targetPos;
            // new start is current position, new target is offset in opposite direction
            startPos = transform.position;
            targetPos = startPos + (forward ? new Vector3(distance, 0f, 0f) : new Vector3(-distance, 0f, 0f));
        }
    }
}
