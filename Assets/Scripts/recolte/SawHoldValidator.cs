using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SawHoldValidator : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Références")]
    public TouchIndicatorWaveMulti leftHold;
    public TouchIndicatorWaveMulti rightHold;
    public WoodHarvestController harvestController;
    public SawBackAndForthUI sawAnimation;

    [Header("Progression UI")]
    public Image radialProgress; // SawProgress (Image Filled Radial 360)

    [Header("Validation")]
    public float requiredHoldTime = 3f;

    [Header("Debug")]
    public bool requireHolds = true;

    private bool sawTouched = false;
    private float holdTimer = 0f;

    void Start()
    {
        if (radialProgress != null)
        {
            radialProgress.fillAmount = 0f;
            radialProgress.enabled = false; // cachée au départ
        }
    }

    void Update()
    {
        bool holdsValid;

        if (requireHolds)
        {
            holdsValid =
                leftHold != null && rightHold != null &&
                leftHold.isTouched &&
                rightHold.isTouched;
        }
        else
        {
            holdsValid = true;
        }

        // ✅ On progresse uniquement si on touche la scie ET que les holds sont valides
        if (sawTouched && holdsValid)
        {
            holdTimer += Time.deltaTime;

            if (radialProgress != null)
            {
                radialProgress.enabled = true;
                radialProgress.fillAmount = Mathf.Clamp01(holdTimer / requiredHoldTime);
            }

            if (sawAnimation != null)
                sawAnimation.SetBoosted(true);

            if (holdTimer >= requiredHoldTime)
                Finish();
        }
        else
        {
            // ✅ On reset uniquement si on avait commencé à remplir
            if (holdTimer > 0f)
                ResetHold();
            else
            {
                if (radialProgress != null)
                    radialProgress.enabled = false;

                if (sawAnimation != null)
                    sawAnimation.SetBoosted(false);
            }
        }
    }

    private void Finish()
    {
        if (sawAnimation != null)
            sawAnimation.SetBoosted(false);

        if (radialProgress != null)
        {
            radialProgress.fillAmount = 1f;
            radialProgress.enabled = false;
        }

        if (harvestController != null)
            harvestController.FinishHarvest();

        enabled = false;
    }

    private void ResetHold()
    {
        holdTimer = 0f;

        if (radialProgress != null)
        {
            radialProgress.fillAmount = 0f;
            radialProgress.enabled = false;
        }

        if (sawAnimation != null)
            sawAnimation.SetBoosted(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        sawTouched = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        sawTouched = false;
        ResetHold();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        sawTouched = false;
        ResetHold();
    }
}
