using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PalmIndicator : MonoBehaviour
{
    [Header("Références")]
    public Image baseArea;   // Image rectangulaire principale (zone cible)
    public Image wavePrefab; // Modèle d’onde (Image enfant)

    [Header("Animation")]
    public float waveDuration = 1.4f;
    public float waveInterval = 0.9f;
    public float maxWaveScale = 2.0f;

    [Header("Couleurs")]
    public Color idleColor    = new Color(1f, 1f, 1f, 0.5f);
    public Color touchedColor = new Color(0f, 1f, 0f, 0.6f);

    [Header("Détection tranche de main (robuste)")]
    [Tooltip("Diamètre minimal estimé du contact (en pixels écran) pour considérer une tranche/paume.")]
    public float minMajorDiameterPx = 60f;
    [Tooltip("Écart minimal entre les touches (en pixels écran) le long de l’axe le plus long du Rect.")]
    public float minSpanAlongLongAxisPx = 120f;
    [Tooltip("Durée minimale (secondes) pendant laquelle la condition doit rester vraie.")]
    public float minHoldTime = 0.15f;

    [Header("Divers")]
    [Tooltip("Simuler une paume avec la souris (utile en Editor). Clique = actif")]
    public bool simulatePalmWithMouseInEditor = true;

    [HideInInspector] public bool isTouched = false;

    private readonly List<Image> activeWaves = new List<Image>();
    private Coroutine waveCoroutine;
    private float holdTimer = 0f;
    private Camera uiCam; // pour Canvas Screen Space - Camera éventuellement

    void Awake()
    {
        if (baseArea == null) baseArea = GetComponent<Image>();
        // Si ton Canvas est en Screen Space - Camera, assigne la camera :
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            uiCam = canvas.worldCamera;
        else
            uiCam = null; // Overlay ou World → pas besoin
    }

    void Start()
    {
        baseArea.color = idleColor;
        if (wavePrefab != null)
        {
            // Si tu as laissé le modèle visible dans la scène, on peut le masquer
            wavePrefab.gameObject.SetActive(false);
        }
        waveCoroutine = StartCoroutine(WaveLoop());
    }

    void Update()
    {
        bool palmConditionNow = DetectPalmEdge();

        // Maintien temporel pour robustesse
        if (palmConditionNow) holdTimer += Time.deltaTime;
        else                  holdTimer  = 0f;

        bool active = holdTimer >= minHoldTime;

        // Changement d’état
        if (active != isTouched)
        {
            isTouched = active;
            if (isTouched)
                Debug.Log("🖐️ Palm edge detected");
        }

        // Transition de couleur fluide
        Color target = isTouched ? touchedColor : idleColor;
        baseArea.color = Color.Lerp(baseArea.color, target, Time.deltaTime * 5f);

        foreach (var wave in activeWaves)
        {
            if (wave == null) continue;
            var c = wave.color;
            c.r = Mathf.Lerp(c.r, target.r, Time.deltaTime * 5f);
            c.g = Mathf.Lerp(c.g, target.g, Time.deltaTime * 5f);
            c.b = Mathf.Lerp(c.b, target.b, Time.deltaTime * 5f);
            wave.color = c;
        }
    }

    bool DetectPalmEdge()
    {
#if UNITY_EDITOR
        // Simulation simple à la souris pour tester en Editor
        if (simulatePalmWithMouseInEditor && Input.GetMouseButton(0))
        {
            Vector2 mouse = Input.mousePosition;
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    baseArea.rectTransform, mouse, uiCam))
            {
                return true;
            }
        }
#endif

        // 1) Récupère toutes les touches dans la zone du Rect
        var touchesInRect = new List<Touch>();
        foreach (var t in Input.touches)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    baseArea.rectTransform, t.position, uiCam))
            {
                touchesInRect.Add(t);
            }
        }

        if (touchesInRect.Count == 0)
            return false;

        // 2) Critère A — taille/diamètre du contact (si disponible)
        // Unity Touch.radius est le rayon estimé en pixels, dépend plateforme.
        // Si l’OS fusionne la paume en un seul contact "large", ce test suffit.
        foreach (var t in touchesInRect)
        {
            // Touch.radius peut être 0 sur certaines plateformes (alors on ignore)
            float diameter = t.radius > 0f ? (t.radius * 2f) : 0f;
            if (diameter >= minMajorDiameterPx)
                return true;
        }

        // 3) Critère B — étendue des touches le long de l’axe principal de la zone
        // Si plusieurs doigts sont vus, on vérifie l'écart le long de l’axe long
        if (touchesInRect.Count >= 2)
        {
            // Détermine quel est l'axe dominant de la zone (largeur vs hauteur)
            bool horizontalLongAxis = baseArea.rectTransform.rect.width >= baseArea.rectTransform.rect.height;

            float minA = float.PositiveInfinity;
            float maxA = float.NegativeInfinity;

            foreach (var t in touchesInRect)
            {
                float a = horizontalLongAxis ? t.position.x : t.position.y;
                if (a < minA) minA = a;
                if (a > maxA) maxA = a;
            }

            float span = maxA - minA;
            if (span >= minSpanAlongLongAxisPx)
                return true;
        }

        // 4) Sinon, conditions non réunies
        return false;
    }

    IEnumerator WaveLoop()
    {
        while (true)
        {
            if (wavePrefab != null && baseArea != null)
            {
                Image wave = Instantiate(wavePrefab, baseArea.transform);
                wave.gameObject.SetActive(true);
                var rt = wave.rectTransform;
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;
                wave.color = baseArea.color;
                wave.raycastTarget = false;
                wave.transform.SetAsFirstSibling(); // derrière la base

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
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (t < waveDuration)
        {
            t += Time.deltaTime;
            float k = t / waveDuration;
            wave.rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, maxWaveScale, k);
            wave.color = Color.Lerp(startColor, endColor, k);
            yield return null;
        }

        activeWaves.Remove(wave);
        if (wave != null) Destroy(wave.gameObject);
    }
}
