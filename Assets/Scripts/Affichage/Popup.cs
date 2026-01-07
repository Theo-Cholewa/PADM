using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    public float Duration;
    public Vector3 Movement;
    public List<Text> Texts;
    public AudioSource Source;

    private float CurrentTime=0f;

    public void SetText(string value)
    {
        foreach(var text in Texts)
        {
            text.text = value;
        }
    }

    public void PlaySound()
    {
        Source.Play();   
    }

    void Update()
    {
        transform.position += Movement * Time.deltaTime;
        CurrentTime += Time.deltaTime;
        if(CurrentTime >= Duration)
        {
            Destroy(gameObject);
        }
    }

    public static void Spawn(Transform transform, GameObject UpPrefab, GameObject DownPrefab, int offset, bool hasSound=true)
    {
        if (offset > 0)
        {
            var effect = Instantiate(UpPrefab, transform);
            effect.GetComponent<Popup>().SetText("+"+offset.ToString());
            if(hasSound) effect.GetComponent<Popup>().PlaySound();
        }
        else if (offset < 0)
        {
            var effect = Instantiate(DownPrefab, transform);
            effect.GetComponent<Popup>().SetText("-"+(-offset).ToString());
            if(hasSound) effect.GetComponent<Popup>().PlaySound();
        }
    }

    private static IEnumerator PwompAnimation(Transform transform)
    {
        for(float i=0f; i<.25f; i+=Time.deltaTime)
        {
            var animation = Mathf.Sin(i * Mathf.PI * 4)*.25f;
            transform.localScale = new Vector3(1 + animation, 1 + animation, 1 + animation);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    public static void Pwomp(MonoBehaviour obj, Transform transform)
    {
        obj.StartCoroutine(PwompAnimation(transform));
    }

}
