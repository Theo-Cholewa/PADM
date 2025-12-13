using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class TouchIndicatorWaveMulti : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, ICancelHandler
{
    [Header("Références")]
    public Image baseCircle;
    public List<Image> wavePrefabs;

    [Header("Animation")]
    public float waveDuration = 1.2f;
    public float waveInterval = 0.8f;
    public float maxWaveScale = 2.5f;

    [Header("Transparence des ondes")]
    [Range(0f, 1f)] public float waveAlphaStart = 0.35f;
    [Range(0f, 1f)] public float waveAlphaEnd = 0f;

    [Header("Couleurs")]
    public Color idleColor = new Color(1f, 1f, 1f, 0.2f);
    public Color touchedColor = new Color(0f, 1f, 0f, 0.2f);

    [Header("Multi-touch")]
    [Min(1)] public int requiredFingers = 3;

    [HideInInspector] public bool isTouched = false;

    private Coroutine waveCoroutine;
    private readonly List<Image> activeWaves = new List<Image>();

    private readonly HashSet<int> activePointers = new HashSet<int>();

    void Start()
    {
        if (baseCircle != null)
            baseCircle.color = idleColor;

        foreach (var prefab in wavePrefabs)
        {
            if (prefab != null)
                prefab.color = idleColor;
        }

        waveCoroutine = StartCoroutine(WaveLoop());
    }

    IEnumerator WaveLoop()
    {
        while (true)
        {
            foreach (var prefab in wavePrefabs)
            {
                if (prefab == null || baseCircle == null) continue;

                Image wave = Instantiate(prefab, baseCircle.transform);
                wave.rectTransform.localScale = Vector3.one;

                Color waveColor = baseCircle.color;
                waveColor.a = waveAlphaStart;
                wave.color = waveColor;

                wave.raycastTarget = false;
                activeWaves.Add(wave);

                StartCoroutine(AnimateWave(wave));
            }

            yield return new WaitForSeconds(waveInterval);
        }
    }

    IEnumerator AnimateWave(Image wave)
    {
        float t = 0f;
        Color startColor = wave.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, waveAlphaEnd);

        while (t < waveDuration)
        {
            t += Time.deltaTime;
            float progress = t / waveDuration;

            wave.rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, maxWaveScale, progress);
            wave.color = Color.Lerp(startColor, endColor, progress);

            yield return null;
        }

        activeWaves.Remove(wave);
        Destroy(wave.gameObject);
    }

    void Update()
    {
        Color targetColor = isTouched ? touchedColor : idleColor;

        if (baseCircle != null)
            baseCircle.color = Color.Lerp(baseCircle.color, targetColor, Time.deltaTime * 5f);

        foreach (var prefab in wavePrefabs)
        {
            if (prefab != null)
                prefab.color = Color.Lerp(prefab.color, targetColor, Time.deltaTime * 5f);
        }

        foreach (var wave in activeWaves)
        {
            if (wave != null)
            {
                Color current = wave.color;
                current.r = Mathf.Lerp(current.r, targetColor.r, Time.deltaTime * 5f);
                current.g = Mathf.Lerp(current.g, targetColor.g, Time.deltaTime * 5f);
                current.b = Mathf.Lerp(current.b, targetColor.b, Time.deltaTime * 5f);
                wave.color = current;
            }
        }
    }

    // -----------------------
    // Multi-touch
    // -----------------------

    public void OnPointerDown(PointerEventData eventData)
    {
        activePointers.Add(eventData.pointerId);
        RefreshTouchedState();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // DEBUG : tu as demandé de laisser désactivé
        activePointers.Remove(eventData.pointerId);
        RefreshTouchedState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // DEBUG : tu as demandé de laisser désactivé
        activePointers.Remove(eventData.pointerId);
        RefreshTouchedState();
    }

    public void OnCancel(BaseEventData eventData)
    {
        // sécurité (perte de focus)
        activePointers.Clear();
        RefreshTouchedState();
    }

    /// <summary>
    /// Active/désactive la réception des touches (sans désactiver le GameObject).
    /// ⚠️ IMPORTANT : on ne clear PAS les doigts ici, sinon flicker.
    /// </summary>
    public void SetRaycast(bool enabled)
    {
        if (baseCircle != null)
            baseCircle.raycastTarget = enabled;
    }

    public void ResetTouches()
    {
        activePointers.Clear();
        RefreshTouchedState();
    }

    private void OnDisable()
    {
        // Si tu désactives vraiment le GO, on clear.
        activePointers.Clear();
        RefreshTouchedState();
    }

    private void RefreshTouchedState()
    {
        isTouched = activePointers.Count >= requiredFingers;
    }
}
