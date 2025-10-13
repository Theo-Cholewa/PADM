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

    void Start()
    {
        UpdateMaterials();
    }

    /// <summary>
    /// Vérifie si la ressource est encore disponible.
    /// </summary>
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

    /// <summary>
    /// Retire une ressource et met à jour la visibilité des plans.
    /// </summary>
    public void CollectResource(string resourceType)
    {
        switch (resourceType)
        {
            case "food":
                hasFood = false;
                break;
            case "wood":
                hasWood = false;
                break;
            case "stone":
                hasStone = false;
                break;
        }

        UpdateMaterials();
    }

    /// <summary>
    /// Active ou désactive les plans (MeshRenderer) selon les ressources restantes.
    /// </summary>
    private void UpdateMaterials()
    {
        if (foodRenderer != null)
            foodRenderer.enabled = hasFood;

        if (woodRenderer != null)
            woodRenderer.enabled = hasWood;

        if (stoneRenderer != null)
            stoneRenderer.enabled = hasStone;
    }
}
