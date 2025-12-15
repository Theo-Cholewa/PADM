using UnityEngine;

public class WoodHarvestController : MonoBehaviour
{
    [Header("Premiers indicateurs")]
    public GameObject[] firstIndicators;

    [Header("Références enfants (PHASE 1)")]
    public TouchIndicatorWaveMulti[] indicators;   // hand-spike-touch-indicator (3 doigts chacun)

    [Header("Objets à activer quand les deux joueurs sont prêts (PHASE 2)")]
    public GameObject[] harvestObjects;            // Saw + hold-touch-indicator + progress, etc.

    [Header("Objets à désactiver quand les deux joueurs sont prêts (PHASE 1 UI)")]
    public GameObject[] triggerObjects;

    [Header("Prefab à masquer quand on valide")]
    public GameObject prefabToHide;

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
        ActivateTriggerObjects(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetAll();
            return;
        }

        if (finished) return;

        // ✅ Une fois en phase 2, on ne gère plus la phase 1 ici.
        // La fin sera déclenchée par SawHoldValidator -> FinishHarvest()
        if (phase2Active) return;

        if (indicators == null || indicators.Length == 0)
            return;

        // PHASE 1 : attendre que tous les indicateurs soient validés (3 doigts)
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

            ActivateHarvestObjects(true);     // affiche Saw + holds + cercle
            ActivateTriggerObjects(false);    // cache la phase 1

            Debug.Log("🌲 Phase 2 activée ! (holds 2 doigts + saw 3s)");
        }
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

        Debug.Log("🔄 Reset récolte (retour phase 1).");
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
            obj?.SetActive(state);
    }

    public void SetLinkedShip(ShipController ship)
    {
        linkedShip = ship;
    }

    // ✅ Appelée par SawHoldValidator quand les 3 secondes sont atteintes
    public void FinishHarvest()
    {
        if (finished) return;
        finished = true;

        ActivateHarvestObjects(false);
        ActivateTriggerObjects(false);
        prefabToHide?.SetActive(false);

        if (linkedShip != null)
        {
            ShipData shipData = linkedShip.GetComponent<ShipData>();
            if (shipData != null)
            {
                shipData.AddResource("wood", 3);
                RessourceClient.current
                    .Get(linkedShip.team)
                    .Add(ResourceType.Wood, 3);

                Debug.Log($"🌲 {linkedShip.team} a récolté du bois !");
            }
        }
        else
        {
            Debug.LogWarning("⚠ Aucun bateau lié pour recevoir le bois !");
        }
    }
}
