using UnityEngine;

public class ShipManager : MonoBehaviour
{
    public static ShipManager Instance { get; private set; }

    [Header("Données des bateaux")]
    public ShipData redShip = new ShipData();
    public ShipData blueShip = new ShipData();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeShips();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeShips()
    {
        redShip.shipColor = Color.red;

        blueShip.shipColor = Color.cyan;
    }

    // --- Méthodes globales ---
    public ShipData GetShip(string name)
    {
        switch (name.ToLower())
        {
            case "red": return redShip;
            case "blue": return blueShip;
            default: Debug.LogWarning($"Nom de bateau inconnu : {name}"); return null;
        }
    }
}