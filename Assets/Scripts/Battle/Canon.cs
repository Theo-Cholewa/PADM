using UnityEngine;

public class Canon : MonoBehaviour
{
    public TeamEnum team;
    public GameObject projectile;

    public AudioClip ShotSound;

    private Transform background;

    public string ammunition;

    public float MinimumPower = 2f;
    public float MaximumPower = 4f;

    private bool isLoaded = false;

    private RessourceClient.TeamClient client;

    // Start is called before the first frame update
    void Start()
    {
        background = transform.GetChild(0);
        client = RessourceClient.current.Get(Team.Of(team));
    }

    public void TryToShoot(float LocalPower)
    {
        var powerLevel = client.value?.cannonLevel ?? 1;
        if(powerLevel<=0) powerLevel = 1;
        var RealPower = MinimumPower + LocalPower * (MaximumPower - MinimumPower) * (.5f+powerLevel/2f);
        if (isLoaded)
        {
            var instance = Instantiate(projectile);
            instance.transform.parent = transform;
            instance.transform.localScale = new(.3f, .3f, .3f);
            instance.transform.localPosition = new(0f, 7f, 0f);
            instance.transform.parent = null;

            instance.GetComponent<Physic>().velocity = transform.TransformVector(new(0f, 7f, 0f)).normalized * RealPower;

            isLoaded = false;
            background.localScale = Vector3.Scale(background.localScale, new Vector3(1f, 1f, 1f / 1.2f));

            GetComponent<AudioSource>().PlayOneShot(ShotSound);
        }
    }

    void OnActivate(float strength)
    {
        TryToShoot(strength);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.body == null) return;
        
        var other = collision.body.gameObject;
        if (other.TryGetComponent<Tagged>(out var ammunition) && ammunition.Tag.Contains(this.ammunition) && !isLoaded)
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
