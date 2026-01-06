using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RessourceCounter : MonoBehaviour
{
    public GameObject Icon;
    public Text Text;
    public GameObject PopupPrefab;
    public GameObject PodownPrefab;

    private int CurrentCount = 1;

    void Start()
    {
        CurrentCount = 0;
        Text.text = "x0";
    }

    public void SetCount(int count)
    {
        // Offset
        var offset = count-CurrentCount;
        Popup.Spawn(Text.transform, PopupPrefab, PodownPrefab, offset);

        CurrentCount = count;

        // Set text
        Text.text = $"x{count}";

        // Animate
        StartCoroutine(Pwomp());
    }

    IEnumerator Pwomp()
    {
        for(float i=0f; i<.25f; i+=Time.deltaTime)
        {
            var animation = Mathf.Sin(i * Mathf.PI * 4)*.25f;
            Icon.transform.localScale = new Vector3(1 + animation, 1 + animation, 1 + animation);
            yield return null;
        }
        Icon.transform.localScale = Vector3.one;
    }
}
