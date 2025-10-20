using UnityEngine;

/*
public class SmoothDrag : MonoBehaviour
{
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 offset;
    private float zCoord;
    private int activeTouchId = -1;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        int touchCount = Input.touchCount;

        if (touchCount == 4)
        {
            for (int i = 0; i < 4; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (!isDragging && touch.phase == TouchPhase.Began)
                {
                    Ray ray = mainCamera.ScreenPointToRay(touch.position);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit) && hit.transform == transform)
                    {
                        zCoord = mainCamera.WorldToScreenPoint(transform.position).z;
                        offset = transform.position - GetWorldPosition(touch.position);
                        isDragging = true;
                        activeTouchId = touch.fingerId;
                        break;
                    }
                }

                if (isDragging && touch.fingerId == activeTouchId)
                {
                    if (touch.phase == TouchPhase.Moved)
                    {
                        transform.position = GetWorldPosition(touch.position) + offset;
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        isDragging = false;
                        activeTouchId = -1;
                    }
                }
            }
        }
        else
        {
            if (isDragging)
            {
                isDragging = false;
                activeTouchId = -1;
            }
        }
    }

    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        Vector3 point = new Vector3(screenPosition.x, screenPosition.y, zCoord);
        return mainCamera.ScreenToWorldPoint(point);
    }
}
*/

public class SmoothDrag : MonoBehaviour
{
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 offset;
    private float zCoord;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void OnMouseDown()
    {
        zCoord = mainCamera.WorldToScreenPoint(transform.position).z;
        offset = transform.position - GetMouseWorldPosition();
        isDragging = true;
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            transform.position = GetMouseWorldPosition() + offset;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return mainCamera.ScreenToWorldPoint(mousePoint);
    }
}
