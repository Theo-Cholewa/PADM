using UnityEngine;

public class Box : MonoBehaviour
{

    public TeamEnum TeamId;

    public GameObject Created;

    public TextMesh CountDisplay;

    public int Cost;

    public RessourceType Material;

    private GameObject content;
        
    private int refillTime =0;
    private RessourceClient.TeamClient ressources;

    void Start()
    {
        ressources = RessourceClient.current.Get(Team.Of(TeamId));
        ressources.onChange.AddListener(RecalculateCount);
        RecalculateCount();
        refill();
    }

    void Destroy()
    {
        ressources.onChange.RemoveListener(RecalculateCount);
    }

    void refill()
    {
        content = Instantiate(Created);
        content.transform.parent = gameObject.transform;
        content.transform.localPosition = new(0, 0, -0.01f);
        content.transform.localScale = new(.8f, .8f, .8f);
        content.transform.localRotation = new();
        content.transform.parent = null;

        content.GetComponent<Pullable>().onTake = () =>
        {
            // Check if has enougth ressources
            var count = (ressources.value?.Get(Material)??100)/Cost;

            if (count <= 0)
            {
                Destroy(content);
            }
            else
            {
                content.GetComponent<Physic>().hasPhysic = true;
                content.GetComponent<Pullable>().onTake = null;   
                ressources.Add(Material, -Cost);
            }
            refillTime = 1;
        };

        content.GetComponent<Physic>().hasPhysic = false;
        refillTime = 0;
    }

    void FixedUpdate()
    {
        if (refillTime > 0)
        {
            refillTime++;
            if (refillTime > 100)
            {
                refill();
            }
        }
    }

    public void RecalculateCount()
    {
        var count = (ressources.value?.Get(Material)??100)/Cost;
        CountDisplay.text = count.ToString();
    }
}
