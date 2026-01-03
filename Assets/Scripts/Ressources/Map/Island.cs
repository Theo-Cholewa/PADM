using UnityEngine;

public class Island : MonoBehaviour
{
    [Header("Identification")]
    public int islandID = 1;

    [Header("Visibilité / Éléments")]
    [Tooltip("True = île visitée (active tout sauf le canvas ressource)")]
    public bool isVisited = false;

    [Tooltip("Contenu principal de l'île (poulailler, décor, etc.)")]
    public GameObject islandContent;

    [Tooltip("Canvas affiché avant visite (icône de ressource ou panneau indicatif)")]
    public Canvas resourceCanvas;

    public enum RessourceType
    {
        None,
        Food,
        Wood,
        Stone,
        Shop,
    }
    [Header("Type de ressource principale")]
    [Tooltip("Choisissez la ressource principale de cette île")]
    public RessourceType mainResource = RessourceType.None;

    void Start()
    {
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
}
