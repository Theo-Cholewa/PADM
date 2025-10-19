using UnityEngine;
using System.Collections.Generic;

public class ChickenSpawner : MonoBehaviour
{
    [Header("Zone de génération (cercle)")]
    public float spawnRadius = 10f;

    [Header("Paramètres de génération")]
    public float spawnInterval = 1f;        // une apparition par seconde
    public int maxChickens = 10;            // nombre max simultané
    public GameObject chickenPrefab;

    [Header("Hauteur du sol (Y)")]
    public float groundY = 0f;

    private float timer = 0f;
    private List<GameObject> activeChickens = new List<GameObject>();

    void Update()
    {
        timer += Time.deltaTime;

        // Une apparition toutes les "spawnInterval" secondes
        if (timer >= spawnInterval)
        {
            timer = 0f;
            TrySpawnChicken();
        }

        // Nettoyage des poulets détruits
        for (int i = activeChickens.Count - 1; i >= 0; i--)
        {
            if (activeChickens[i] == null)
                activeChickens.RemoveAt(i);
        }
    }

    void TrySpawnChicken()
    {
        // Vérifie la limite max
        if (activeChickens.Count >= maxChickens)
            return;

        Vector2 randomPos = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = new Vector3(randomPos.x, groundY, randomPos.y) + transform.position;

        // Rotation aléatoire sur l’axe Y
        Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        GameObject newChicken = Instantiate(chickenPrefab, spawnPos, randomRotation, transform);
        activeChickens.Add(newChicken);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.4f);
        Gizmos.DrawSphere(transform.position, spawnRadius);
    }
#endif
}
