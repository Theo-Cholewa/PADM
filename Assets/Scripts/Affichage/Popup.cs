using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    public float Duration;
    public Vector3 Movement;
    public List<Text> Texts;

    private float CurrentTime=0f;

    public void SetText(string value)
    {
        foreach(var text in Texts)
        {
            text.text = value;
        }
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

    public static void Spawn(Transform transform, GameObject UpPrefab, GameObject DownPrefab, int offset)
    {
        if (offset > 0)
        {
            var effect = Instantiate(UpPrefab, transform);
            effect.GetComponent<Popup>().SetText("+"+offset.ToString());
        }
        else if (offset < 0)
        {
            var effect = Instantiate(DownPrefab, transform);
            effect.GetComponent<Popup>().SetText("-"+(-offset).ToString());
        }
    }

}
