using UnityEngine;

public class Canon : MonoBehaviour
{
    public GameObject projectile;

    private Transform background;
    private float baseWidth;

    public string ammunition;

    public float power=1f;

    private bool isLoaded = false;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Canon ready");
        background = transform.GetChild(0);
        baseWidth = background.localScale.x;
    }

    void OnTouchDown(TouchInfo info)
    {
        if (isLoaded)
        {
            var ptransform = transform;

            Debug.Log("Clicked on the canon");
            var spawn = ptransform.position + ptransform.up * 5 * ptransform.lossyScale.y;

            var instance = Instantiate(projectile);
            instance.transform.parent = transform;
            instance.transform.localScale = new(.3f,.3f,.3f);
            instance.transform.localPosition = new(0f, 7f, 0f);
            instance.transform.parent = null;

            instance.GetComponent<Physic>().velocity = transform.TransformVector(new(0f, 7f, 0f)).normalized * power;

            isLoaded = false;
            background.localScale = Vector3.Scale(background.localScale, new Vector3(1f, 1f, 1f/1.2f));
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collide");
        var ammunition = collision.body.gameObject.GetComponent<Ammunition>();
        Debug.Log("List");
        Debug.Log(ammunition.Tag);
        Debug.Log(this.ammunition);
        Debug.Log(ammunition.Tag.Contains(this.ammunition));
        if (ammunition.Tag.Contains(this.ammunition) && !isLoaded)
        {
            isLoaded = true;
            background.localScale = Vector3.Scale(background.localScale, new Vector3(1f, 1f, 1.2f));
            Destroy(ammunition.gameObject);
        }
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
