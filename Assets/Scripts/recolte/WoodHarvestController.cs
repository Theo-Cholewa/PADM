using UnityEngine;

public class WoodHarvestController : MonoBehaviour
{
    [Header("Premiers indicateurs")]
    public GameObject[] firstIndicators;

    [Header("Références enfants (PHASE 1)")]
    public TouchIndicatorWaveMulti[] indicators; // phase 1 (hand-spike)

    [Header("Objets à activer quand les deux joueurs sont prêts (PHASE 2)")]
    public GameObject[] harvestObjects; // contient Saw + hold-touch-indicator + etc.

    [Header("Objets de trigger (optionnel) - UI phase 1")]
    public GameObject[] triggerObjects;

    [Header("Prefab à masquer quand la récolte est finie")]
    public GameObject prefabToHide;

    [Header("Composant dont on veut supprimer un script")]
    public GameObject targetObject;
    public string scriptNameToRemove;

    [Header("🔹 Référence au bateau accosté")]
    private ShipController linkedShip;

    private bool phase2Active = false;
    private bool finished = false;

    void Start()
    {
        if (indicators == null || indicators.Length == 0)
            indicators = GetComponentsInChildren<TouchIndicatorWaveMulti>();

        // Phase 2 OFF au départ
        ActivateHarvestObjects(false);

        // Phase 1 ON
        SetIndicatorsRaycast(true);
        ActivateTriggerObjects(true);
    }

    void Update()
    {
        // Reset
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetAll();
            return;
        }

        // Si déjà fini, ne plus rien gérer
        if (finished) return;

        if (indicators == null || indicators.Length == 0)
            return;

        // PHASE 1 : attendre que les 2 indicateurs soient OK
        bool everyoneActive = true;
        foreach (var indicator in indicators)
        {
            if (indicator == null || !indicator.isTouched)
            {
                everyoneActive = false;
                break;
            }
        }

        if (everyoneActive && !phase2Active)
        {
            phase2Active = true;

            // Active phase 2
            ActivateHarvestObjects(true);

            // Cache phase 1
            ActivateTriggerObjects(false);

            // On empêche de retoucher phase 1 (mais on ne reset pas l'état)
            SetIndicatorsRaycast(false);

            Debug.Log("🌲 Phase 2 activée !");
        }

        // Si tu veux garder le retour phase 1 si quelqu'un relâche, garde ça.
        // Sinon, commente ce bloc.
        if (!everyoneActive && phase2Active)
        {
            phase2Active = false;

            ActivateHarvestObjects(false);
            ActivateTriggerObjects(true);
            SetIndicatorsRaycast(true);

            // Reset aussi les touches phase 2 (sinon états collés)
            ResetTouchesInHarvestObjects();

            Debug.Log("❌ Retour phase 1.");
        }
    }

    // ✅ Appelée par SawHoldFinish quand la découpe est terminée
    public void FinishHarvest()
    {
        if (finished) return;
        finished = true;

        ActivateHarvestObjects(false);
        ActivateTriggerObjects(false);
        SetIndicatorsRaycast(false);
        prefabToHide?.SetActive(false);

        // Récompense
        if (linkedShip != null)
        {
            ShipData shipData = linkedShip.GetComponent<ShipData>();
            if (shipData != null)
            {
                shipData.AddResource("wood", 10);
                Debug.Log($"🌲 {linkedShip.playerName} a récolté du bois — total bois : {shipData.wood}");
            }
        }
        else
        {
            Debug.LogWarning("⚠ Aucun bateau lié pour recevoir le bois !");
        }

        // Suppression script optionnelle
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

        Debug.Log("✅ Wood-harvest terminé !");
    }

    private void ResetAll()
    {
        // Reset touches phase 1
        if (indicators != null)
        {
            foreach (var ind in indicators)
                ind?.ResetTouches();
        }

        finished = false;
        phase2Active = false;

        ActivateHarvestObjects(false);
        ActivateTriggerObjects(true);
        SetIndicatorsRaycast(true);

        ResetTouchesInHarvestObjects();

        Debug.Log("🔄 Reset complet (phase 1).");
    }

    void ActivateHarvestObjects(bool state)
    {
        if (harvestObjects == null) return;
        foreach (var obj in harvestObjects)
            obj?.SetActive(state);
    }

    void ActivateTriggerObjects(bool state)
    {
        if (triggerObjects == null) return;

        foreach (var obj in triggerObjects)
        {
            if (obj == null) continue;

            var ind = obj.GetComponent<TouchIndicatorWaveMulti>();
            if (ind != null)
            {
                ind.SetRaycast(state);

                var cg = obj.GetComponent<CanvasGroup>();
                if (cg == null) cg = obj.AddComponent<CanvasGroup>();

                cg.alpha = state ? 1f : 0f;
                cg.interactable = state;
                cg.blocksRaycasts = state;

                continue;
            }

            obj.SetActive(state);
        }
    }

    void SetIndicatorsRaycast(bool enabled)
    {
        if (indicators == null) return;
        foreach (var ind in indicators)
            ind?.SetRaycast(enabled);
    }

    private void ResetTouchesInHarvestObjects()
    {
        if (harvestObjects == null) return;

        foreach (var go in harvestObjects)
        {
            if (go == null) continue;

            // reset tous les TouchIndicatorWaveMulti trouvés dans les enfants
            var touches = go.GetComponentsInChildren<TouchIndicatorWaveMulti>(true);
            foreach (var t in touches)
                t?.ResetTouches();
        }
    }

    public void SetLinkedShip(ShipController ship)
    {
        linkedShip = ship;
    }
}
