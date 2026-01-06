using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    public float Duration;
    public Vector3 Movement;

    private float CurrentTime=0f;

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
            effect.GetComponentInChildren<Text>().text = "+"+offset.ToString();
        }
        else if (offset < 0)
        {
            var effect = Instantiate(DownPrefab, transform);
            effect.GetComponentInChildren<Text>().text = "-"+(-offset).ToString();
        }
    }

}
