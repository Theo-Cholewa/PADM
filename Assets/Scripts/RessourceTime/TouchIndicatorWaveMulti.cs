using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class TouchIndicatorWaveMulti : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Références")]
    public Image baseCircle;               // Le cercle principal fixe
    public List<Image> wavePrefabs;        // Liste d'images à animer en onde

    [Header("Animation")]
    public float waveDuration = 1.2f;      // Durée de vie d'une onde
    public float waveInterval = 0.8f;      // Délai entre deux ondes
    public float maxWaveScale = 2.5f;      // Taille maximale d'une onde

    [Header("Transparence des ondes")]
    [Range(0f, 1f)] public float waveAlphaStart = 0.35f;
    [Range(0f, 1f)] public float waveAlphaEnd = 0f;

    [Header("Couleurs")]
    public Color idleColor = new Color(1f, 1f, 1f, 0f);
    public Color touchedColor = new Color(0f, 1f, 0f, 0f);

    [HideInInspector]
    public bool isTouched = false;
    private Coroutine waveCoroutine;
    private readonly List<Image> activeWaves = new List<Image>();

    void Start()
    {
        if (baseCircle != null)
            baseCircle.color = idleColor;

        // Initialisation de la couleur de chaque prefab
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
            // Pour chaque image de la liste, lancer une onde
            foreach (var prefab in wavePrefabs)
            {
                if (prefab == null) continue;

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
        // Détermine la couleur cible
        Color targetColor = isTouched ? touchedColor : idleColor;

        // Change la couleur du cercle principal
        if (baseCircle != null)
            baseCircle.color = Color.Lerp(baseCircle.color, targetColor, Time.deltaTime * 5f);

        // Applique la couleur aux prefabs de base
        foreach (var prefab in wavePrefabs)
        {
            if (prefab != null)
                prefab.color = Color.Lerp(prefab.color, targetColor, Time.deltaTime * 5f);
        }

        // Applique aussi aux ondes actives
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

        if (baseCircle != null)
        {
            // Ajuste l'alpha du cercle principal
            Color baseColor = baseCircle.color;
            baseColor.a = 0.2f;
            baseCircle.color = baseColor;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isTouched = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isTouched = true;
    }
}
