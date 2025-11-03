using UnityEngine;

public class Island : MonoBehaviour
{
    [Header("Identification")]
    public int islandID = 1;

    [Header("Disponibilité des ressources")]
    public bool hasFood = true;
    public bool hasWood = true;
    public bool hasStone = true;

    [Header("Plans / MeshRenderer associés aux ressources")]
    public Renderer foodRenderer;
    public Renderer woodRenderer;
    public Renderer stoneRenderer;

    [Header("Visibilité / Éléments")]
    [Tooltip("True = île visitée (active tout sauf le canvas ressource)")]
    public bool isVisited = false;

    [Tooltip("Contenu principal de l'île (poulailler, décor, etc.)")]
    public GameObject islandContent;

    [Tooltip("Canvas affiché avant visite (icône de ressource ou panneau indicatif)")]
    public Canvas resourceCanvas;

    void Start()
    {
        UpdateMaterials();
        UpdateVisibility();
    }

    public void SetVisited(bool state)
    {
        if (isVisited == state) return;

        isVisited = state;
        UpdateVisibility();

        if (isVisited)
            Debug.Log($"🌴 L'île {islandID} a été visitée !");
    }

    private void UpdateVisibility()
    {
        // Si non visitée → on affiche uniquement le canvas ressource
        if (!isVisited)
        {
            if (islandContent != null)
                islandContent.SetActive(false);

            if (resourceCanvas != null)
                resourceCanvas.gameObject.SetActive(true);
        }
        // Si visitée → on affiche tout sauf le canvas ressource
        else
        {
            if (islandContent != null)
                islandContent.SetActive(true);

            if (resourceCanvas != null)
                resourceCanvas.gameObject.SetActive(false);
        }
    }

    public bool HasResource(string resourceType)
    {
        return resourceType switch
        {
            "food" => hasFood,
            "wood" => hasWood,
            "stone" => hasStone,
            _ => false,
        };
    }

    public void CollectResource(string resourceType)
    {
        switch (resourceType)
        {
            case "food": hasFood = false; break;
            case "wood": hasWood = false; break;
            case "stone": hasStone = false; break;
        }

        UpdateMaterials();
    }

    private void UpdateMaterials()
    {
        if (foodRenderer != null) foodRenderer.enabled = hasFood;
        if (woodRenderer != null) woodRenderer.enabled = hasWood;
        if (stoneRenderer != null) stoneRenderer.enabled = hasStone;
    }
}
