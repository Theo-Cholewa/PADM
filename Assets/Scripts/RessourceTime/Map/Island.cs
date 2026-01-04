using Unity.VisualScripting;
using UnityEngine;

public class Island : MonoBehaviour
{
    public string Name = "Unnamed";

    [Tooltip("Contenu principal de l'île (poulailler, décor, etc.)")]
    public GameObject DockedContent;

    [Tooltip("Canvas affiché avant visite (icône de ressource ou panneau indicatif)")]
    public Canvas UndockedContent;

    public Object BehaviourObject;

    public IslandBehaviour Behaviour => BehaviourObject?.GetComponent<IslandBehaviour>();

    [HideInInspector]
    public bool IsDocked = false;

    void Start()
    {
        UpdateVisibility();
    }

    public void SetDocked(bool state)
    {
        if (IsDocked == state) return;

        IsDocked = state;
        UpdateVisibility();

        if (IsDocked) Debug.Log($"🌴 L'île {Name} a été visitée !");
    }

    private void UpdateVisibility()
    {
        // Si non visitée → on affiche uniquement le canvas ressource
        if (!IsDocked)
        {
            if (DockedContent != null)
                DockedContent.SetActive(false);

            if (UndockedContent != null)
                UndockedContent.gameObject.SetActive(true);
        }
        // Si visitée → on affiche tout sauf le canvas ressource
        else
        {
            if (DockedContent != null)
                DockedContent.SetActive(true);

            if (UndockedContent != null)
                UndockedContent.gameObject.SetActive(false);
        }
    }
}
