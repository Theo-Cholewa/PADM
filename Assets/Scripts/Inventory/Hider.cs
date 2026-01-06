using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Hider : MonoBehaviour
{
    public RectTransform Panel;
    public float AnimationDuration;

    void Start()
    {
        Panel.anchorMax = new Vector2(1,2);
        Panel.anchorMin = new Vector2(0,1);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            HideAndShow();
        }
    }

    public Task HideAndShow()
    {
        var completion = new TaskCompletionSource<bool>();
        StartCoroutine(Animation(completion));
        return completion.Task;
    }

    IEnumerator Animation(TaskCompletionSource<bool> completion)
    {
        var startTime = Time.time;
        var resolved = false;

        while (true)
        {
            var current = Time.time-startTime;
            if(current>AnimationDuration)break;

            var advancement = Math.Clamp(current/AnimationDuration, 0f, 1f);

            var animation = 1-Mathf.Sin(advancement * Mathf.PI);

            if (advancement>.5f && !resolved)
            {
                resolved = true;
                completion.SetResult(true);
            }

            Panel.anchorMax = new Vector2(1,1+animation);
            Panel.anchorMin = new Vector2(0,0+animation);

            yield return new WaitForEndOfFrame();
        }

        Panel.anchorMax = new Vector2(1,2);
        Panel.anchorMin = new Vector2(0,1);
    }
}
