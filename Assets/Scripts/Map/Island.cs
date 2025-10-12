using UnityEngine;
using UnityEngine.UI;

public class Island : MonoBehaviour
{
    [Header("Identification")]
    public int islandID = 1;

    [Header("Disponibilité des ressources")]
    public bool hasFood = true;
    public bool hasWood = true;
    public bool hasStone = true;

    [Header("Icônes UI (RawImages au-dessus de l'île)")]
    public RawImage foodIcon;
    public RawImage woodIcon;
    public RawImage stoneIcon;

    void Start()
    {
        UpdateIcons();
    }

    /// <summary>
    /// Indique si l’île possède encore cette ressource.
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
    /// Retire la ressource (après récolte) et met à jour les icônes.
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

        UpdateIcons();
    }

    /// <summary>
    /// Active ou désactive les RawImages selon les ressources encore disponibles.
    /// </summary>
    private void UpdateIcons()
    {
        if (foodIcon != null)
            foodIcon.gameObject.SetActive(hasFood);

        if (woodIcon != null)
            woodIcon.gameObject.SetActive(hasWood);

        if (stoneIcon != null)
            stoneIcon.gameObject.SetActive(hasStone);
    }
}