using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class TouchIndicatorWave : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Références")]
    public Image baseCircle;      // Le cercle principal fixe
    public Image wavePrefab;      // L'image pour les ondes

    [Header("Animation")]
    public float waveDuration = 1.2f;   // Durée de vie d'une onde
    public float waveInterval = 0.8f;   // Délai entre deux ondes
    public float maxWaveScale = 2.5f;   // Taille maximale d'une onde

    [Header("Couleurs")]
    public Color idleColor = new Color(1f, 1f, 1f, 0.6f);
    public Color touchedColor = new Color(0f, 1f, 0f, 0.6f);

    [HideInInspector]
    public bool isTouched = false;
    private Coroutine waveCoroutine;
    private readonly List<Image> activeWaves = new List<Image>();

    void Start()
    {
        baseCircle.color = idleColor;
        waveCoroutine = StartCoroutine(WaveLoop());
    }

    IEnumerator WaveLoop()
    {
        while (true)
        {
            // Crée une nouvelle onde
            Image wave = Instantiate(wavePrefab, baseCircle.transform);
            wave.rectTransform.localScale = Vector3.one;
            wave.color = baseCircle.color;  // couleur actuelle
            wave.raycastTarget = false;

            activeWaves.Add(wave);

            StartCoroutine(AnimateWave(wave));

            yield return new WaitForSeconds(waveInterval);
        }
    }

    IEnumerator AnimateWave(Image wave)
    {
        float t = 0f;
        Color startColor = wave.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

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
        baseCircle.color = Color.Lerp(baseCircle.color, targetColor, Time.deltaTime * 5f);

        // Applique la même couleur à toutes les ondes actives
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

    public void OnPointerDown(PointerEventData eventData)
    {
        isTouched = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isTouched = false;
    }
}
