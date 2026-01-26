using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MultiTouchButton : MonoBehaviour
{
    public UnityEvent onTouchDown;

    private void OnTouchDown()
    {
        onTouchDown.Invoke();
    }
}
