using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CloudRevealController : MonoBehaviour
{
    [Header("Références")]
    public List<Transform> cloudPieces = new List<Transform>();

    [Header("Animation")]
    public float moveDistance = 30f;   // distance d’ouverture sur les côtés
    public float duration = 3f;        // durée de l’ouverture
    public float waitTime = 1f;        // temps avant ouverture
    public bool fadeOut = true;        // fade de transparence
    public bool disableAfterReveal = true;

    private Vector3[] startPositions;
    private Vector3[] directions;
    private Renderer[] renderers;
    private bool isSwitching = false;

    void Start()
    {
        if (cloudPieces.Count == 0)
        {
            foreach (Transform child in transform)
                cloudPieces.Add(child);
        }

        foreach (var cloud in cloudPieces)
        {
            if (cloud != null && !cloud.gameObject.activeSelf)
                cloud.gameObject.SetActive(true);
        }

        int n = cloudPieces.Count;
        startPositions = new Vector3[n];
        directions = new Vector3[n];
        renderers = new Renderer[n];

        // Calcul des directions (dans le plan horizontal)
        Vector3 center = transform.position;
        for (int i = 0; i < n; i++)
        {
            startPositions[i] = cloudPieces[i].position;

            Vector3 dir = cloudPieces[i].position - center;
            dir.y = 0f;
            if (dir.magnitude < 0.1f)
                dir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
            dir.Normalize();
            directions[i] = dir;

            renderers[i] = cloudPieces[i].GetComponentInChildren<Renderer>();
        }

        StartCoroutine(PlayCloudReveal());
    }

    IEnumerator PlayCloudReveal()
    {
        if (isSwitching) yield break;
        isSwitching = true;

        // Attendre avant de commencer
        yield return new WaitForSeconds(waitTime);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;

            for (int i = 0; i < cloudPieces.Count; i++)
            {
                if (cloudPieces[i] == null) continue;

                // Déplacement latéral
                cloudPieces[i].position = Vector3.Lerp(
                    startPositions[i],
                    startPositions[i] + directions[i] * moveDistance,
                    t
                );

                // Fade out progressif
                if (fadeOut && renderers[i] != null)
                {
                    Color c = renderers[i].material.color;
                    c.a = Mathf.Lerp(1f, 0f, t);
                    renderers[i].material.color = c;
                }
            }

            elapsed += Time.deltaTime;
            isSwitching = false;
            yield return null;
        }

        for (int i = 0; i < cloudPieces.Count; i++)
        {
            if (cloudPieces[i] == null) continue;
            cloudPieces[i].position = startPositions[i] + directions[i] * moveDistance;

            if (disableAfterReveal)
                cloudPieces[i].gameObject.SetActive(false);
        }

        isSwitching = false;
    }

    // 🔄 Fonction inverse : fait revenir les nuages sur la scène
    public IEnumerator PlayCloudHide()
    {
        // Réactive les nuages si désactivés
        foreach (var cloud in cloudPieces)
        {
            if (cloud != null && !cloud.gameObject.activeSelf)
                cloud.gameObject.SetActive(true);
        }

        for (int i = 0; i < cloudPieces.Count; i++)
        {
            if (renderers[i] != null)
            {
                Color c = renderers[i].material.color;
                c.a = 0f;
                renderers[i].material.color = c;
            }
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;

            for (int i = 0; i < cloudPieces.Count; i++)
            {
                if (cloudPieces[i] == null) continue;

                // Déplacement inverse (ils reviennent au centre)
                cloudPieces[i].position = Vector3.Lerp(
                    startPositions[i] + directions[i] * moveDistance,
                    startPositions[i],
                    t
                );

                // Fade in progressif
                if (fadeOut && renderers[i] != null)
                {
                    Color c = renderers[i].material.color;
                    c.a = Mathf.Lerp(0f, 1f, t);
                    renderers[i].material.color = c;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Position finale : au centre, opaques
        for (int i = 0; i < cloudPieces.Count; i++)
        {
            if (cloudPieces[i] == null) continue;
            cloudPieces[i].position = startPositions[i];

            if (renderers[i] != null)
            {
                Color c = renderers[i].material.color;
                c.a = 1f;
                renderers[i].material.color = c;
            }
        }
    }
}
