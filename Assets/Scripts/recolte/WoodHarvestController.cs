using UnityEngine;

public class WoodHarvestController : MonoBehaviour
{
    [Header("Premiers indicateurs")]
    public GameObject[] firstIndicators;

    [Header("Références enfants")]
    public TouchIndicatorWaveMulti[] indicators;   // Références vers les deux composants enfants

    [Header("Objets à activer quand les deux joueurs sont prêts")]
    public GameObject[] harvestObjects;

    [Header("Objets à desactiver quand les deux joueurs sont prêts")]
    public GameObject[] triggerObjects;

    [Header("Prefab à masquer quand on appuie sur ESPACE")]
    public GameObject prefabToHide;

    [Header("🔹 Référence au bateau accosté")]
    private ShipController linkedShip;

    private bool allActivated = false;
    /*
    void Awake()
    {
        if (firstIndicators != null)
        {
            foreach (var indicator in firstIndicators)
            {
                if (indicator == null) continue;

                // On tente de désactiver le GameObject
                indicator.SetActive(false);

                // On ajoute aussi une sécurité avec CanvasGroup pour les UI
                var cg = indicator.GetComponent<CanvasGroup>();
                if (cg == null)
                    cg = indicator.AddComponent<CanvasGroup>();

                cg.alpha = 0; // invisible
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
        }
    }*/

    void Start()
    {
        // Si rien n’est assigné manuellement, on récupère automatiquement les enfants
        if (indicators == null || indicators.Length == 0)
            indicators = GetComponentsInChildren<TouchIndicatorWaveMulti>();
        /*
        if (firstIndicators != null)
        {
            // Désactive tous les premiers indicateurs au départ
            foreach (var indicator in firstIndicators)
            {
                indicator?.SetActive(false);
            }
        }*/

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
        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach (var indicator in firstIndicators)
            {
                if (indicator == null) continue;

                indicator.SetActive(true);

                var cg = indicator.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 1;
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                }
            }

            Debug.Log("🔄 Réinitialisation des indicateurs de récolte.");
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ActivateHarvestObjects(false);
            ActivateTriggerObjects(false);
            prefabToHide?.SetActive(false);

            if (linkedShip != null)
            {
                ShipData shipData = linkedShip.GetComponent<ShipData>();
                if (shipData != null)
                {
                    shipData.AddResource("wood", 3);
                    Debug.Log($"🌲 {linkedShip.team} a récolté du bois — total bois : {shipData.wood}");
                    RessourceClient.current.Get(linkedShip.team).Add(RessourceType.Wood, 3);
                }
            }
            else
            {
                Debug.LogWarning("⚠ Aucun bateau lié pour recevoir le bois !");
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

    // 🔹 Lien avec le bateau accosté (appelé depuis ShipController)
    public void SetLinkedShip(ShipController ship)
    {
        linkedShip = ship;
    }

}
