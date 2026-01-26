using UnityEngine;

public class WoodHarvestController : MonoBehaviour, IslandBehaviour
{
    [Header("Premiers indicateurs")]
    public GameObject[] firstIndicators;

    [Header("Références enfants")]
    public TouchIndicatorWaveMulti[] indicators;   // Références vers les deux composants enfants

    [Header("Objets à activer quand les deux joueurs sont prêts")]
    public GameObject[] harvestObjects;

    [Header("Objets à desactiver quand les deux joueurs sont prêts")]
    public GameObject[] triggerObjects;

    [Header("Prefab à gérer à la fin (on ne veut jamais tout faire disparaître)")]
    public GameObject prefabToHide;

    [Header("🔹 Référence au bateau accosté")]
    private ShipController linkedShip;

    private bool allActivated = false;

    void Start()
    {
        // Si rien n’est assigné manuellement, on récupère automatiquement les enfants
        if (indicators == null || indicators.Length == 0)
            indicators = GetComponentsInChildren<TouchIndicatorWaveMulti>();

        // Désactive tous les objets de récolte au départ
        if (harvestObjects != null)
        {
            foreach (var obj in harvestObjects)
                obj?.SetActive(false);
        }

        // Active tous les objets de trigger au départ
        if (triggerObjects != null)
        {
            foreach (var obj in triggerObjects)
                obj?.SetActive(true);
        }
    }

    void Update()
    {
        // Debug reset indicateurs
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetFirstIndicators();
            Debug.Log("🔄 Réinitialisation des indicateurs de récolte.");
            return;
        }

        // Debug : permet de simuler la fin d'animation au clavier
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("🧪 DEBUG: Space -> CompleteHarvest()");
            CompleteHarvest();
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

        // Tous activés -> récolte possible
        if (everyoneActive && !allActivated)
        {
            allActivated = true;
            ActivateHarvestObjects(true);
            ActivateTriggerObjects(false);
        }

        // Un relâche -> on revient à l'état initial
        if (!everyoneActive && allActivated)
        {
            allActivated = false;
            ActivateHarvestObjects(false);
            ActivateTriggerObjects(true);
        }
    }

    // ✅ A appeler à la FIN de l'animation (Animation Event / Timeline)
    public void CompleteHarvest()
    {
        Debug.Log($"✅ CompleteHarvest called | allActivated={allActivated} linkedShip={(linkedShip != null ? linkedShip.team.ToString() : "NULL")}");

        // Reset visuel, mais on garde toujours quelque chose visible
        ActivateHarvestObjects(false);
        ActivateTriggerObjects(true);
        if (prefabToHide != null) prefabToHide.SetActive(true);

        // Don de ressource + envoi icône (à chaque fin d'animation)
        if (linkedShip != null)
        {
            Debug.Log($"🌲 {linkedShip.team} a récolté du bois");
            RessourceClient.current.Get(linkedShip.team).SendIcon(transform.position, RessourceType.Wood);
            RessourceClient.current.Get(linkedShip.team).Add(RessourceType.Wood, 3);
        }
        else
        {
            Debug.LogWarning("⚠ CompleteHarvest: Aucun bateau lié (linkedShip=NULL) -> pas de récompense");
        }

        // Réarmement pour la prochaine boucle
        allActivated = false;
    }

    private void ResetFirstIndicators()
    {
        if (firstIndicators == null) return;

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
    }

    void ActivateHarvestObjects(bool state)
    {
        if (harvestObjects == null) return;

        foreach (var obj in harvestObjects)
            obj?.SetActive(state);

        if (state)
            Debug.Log("🌲 Récolte possible — objets activés !");
        else
            Debug.Log("❌ Récolte interrompue — objets désactivés.");
    }

    void ActivateTriggerObjects(bool state)
    {
        if (triggerObjects == null) return;

        foreach (var obj in triggerObjects)
            obj?.SetActive(state);

        if (state)
            Debug.Log("🔔 Triggers activés.");
        else
            Debug.Log("🔕 Triggers désactivés.");
    }

    // 🔹 Lien avec le bateau accosté (appelé depuis ShipController)
    public void Dock(ShipController ship)
    {
        linkedShip = ship;

        // ✅ reset complet à chaque arrivée
        allActivated = false;
        ActivateHarvestObjects(false);
        ActivateTriggerObjects(true);
        if (prefabToHide != null) prefabToHide.SetActive(true);

        ResetFirstIndicators();

        gameObject.SetActive(true);

        Debug.Log($"🟢 Dock OK: linkedShip={(linkedShip != null ? linkedShip.team.ToString() : "NULL")}");
    }

    public void Undock(ShipController ship)
    {
        // ✅ Important : ignore si ce n'est pas le même bateau que celui lié
        if (linkedShip != null && ship != null && linkedShip != ship)
        {
            Debug.Log($"🟡 Undock ignoré: demandé par {ship.team} mais linkedShip={linkedShip.team}");
            return;
        }

        // ✅ Reset visuel, mais on ne désactive jamais l'île
        allActivated = false;
        ActivateHarvestObjects(false);
        ActivateTriggerObjects(true);
        if (prefabToHide != null) prefabToHide.SetActive(true);

        // on unlink seulement si c'est bien ce bateau
        linkedShip = null;

        Debug.Log("🔴 Undock OK (reset + unlink, île reste active)");
    }
}