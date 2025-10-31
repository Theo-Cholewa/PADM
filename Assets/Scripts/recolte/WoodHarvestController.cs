using UnityEngine;

public class WoodHarvestController : MonoBehaviour
{
    [Header("Références enfants")]
    public TouchIndicatorWaveMulti[] indicators;   // Références vers les deux composants enfants

    [Header("Objets à activer quand les deux joueurs sont prêts")]
    public GameObject[] harvestObjects;

    [Header("Objets à desactiver quand les deux joueurs sont prêts")]
    public GameObject[] triggerObjects;

    [Header("Prefab à masquer quand on appuie sur ESPACE")]
    public GameObject prefabToHide;

    [Header("Composant dont on veut supprimer un script")]
    public GameObject targetObject;
    public string scriptNameToRemove;

    private bool allActivated = false;

    void Start()
    {
        // Si rien n’est assigné manuellement, on récupère automatiquement les enfants
        if (indicators == null || indicators.Length == 0)
            indicators = GetComponentsInChildren<TouchIndicatorWaveMulti>();

        if (harvestObjects != null)
        {
            // Désactive tous les objets de récolte au départ
            foreach (var obj in harvestObjects)
            {
                obj?.SetActive(false);
            }
        }
        
        if (triggerObjects != null)
        {
            // Active tous les objets de trigger au départ
            foreach (var obj in triggerObjects)
            {
                obj?.SetActive(true);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ActivateHarvestObjects(false);
            ActivateTriggerObjects(false);
            prefabToHide?.SetActive(false);

            if (targetObject != null && !string.IsNullOrEmpty(scriptNameToRemove))
            {
                var component = targetObject.GetComponent(scriptNameToRemove);
                if (component != null)
                {
                    if (component is IslandPulse islandPulse && islandPulse.islandRenderer != null)
                    {
                        islandPulse.islandRenderer.material.color = islandPulse.baseColor;
                        Debug.Log("🎨 Couleur de l'île réinitialisée avant suppression du script.");
                    }
                    Destroy(component);
                    Debug.Log($"🗑️ Script '{scriptNameToRemove}' supprimé de {targetObject.name}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Aucun script nommé '{scriptNameToRemove}' trouvé sur {targetObject.name}");
                }
            }
            return;
        }
        
        if (indicators == null || indicators.Length == 0)
            return;

        bool everyoneActive = true;

        // Vérifie si chaque TouchIndicatorWaveMulti est actif
        foreach (var indicator in indicators)
        {
            if (indicator == null || !indicator.isTouched)
            {
                everyoneActive = false;
                break;
            }
        }

        // Si tous sont activés et que ce n’était pas encore le cas → message console
        if (everyoneActive && !allActivated)
        {
            allActivated = true;
            ActivateHarvestObjects(true);
            ActivateTriggerObjects(false);
        }

        // Si un se relâche, on peut repasser à false (facultatif)
        if (!everyoneActive && allActivated)
        {
            allActivated = false;
            ActivateHarvestObjects(false);
            ActivateTriggerObjects(true);
        }
    }

    void ActivateHarvestObjects(bool state)
    {
        if (harvestObjects == null) return;

        foreach (var obj in harvestObjects)
        {
            obj?.SetActive(state);
        }

        if (state)
            Debug.Log("🌲 Récolte possible — objets activés !");
        else
            Debug.Log("❌ Récolte interrompue — objets désactivés.");
    }

    void ActivateTriggerObjects(bool state)
    {
        if (triggerObjects == null) return;

        foreach (var obj in triggerObjects)
        {
            obj?.SetActive(state);
        }

        if (state)
            Debug.Log("🔔 Triggers activés.");
        else
            Debug.Log("🔕 Triggers désactivés.");
    }
}
