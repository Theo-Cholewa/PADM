using UnityEngine;

public class MarketGUI : MonoBehaviour
{
    [Header("Content")]
    public ShopItem Stone;
    public ShopItem Wood;
    public ShopItem Chicken;
    public UnityEngine.UI.Image TitleBackground;

    private Color defaultColor;

    public PartyTools.ValueServer<GameStats> stats;

    void Start()
    {
        defaultColor = TitleBackground.color;

        stats = new(
            Party.current,
            "game_stats",
            new GameStats{
                IsInFight = false,
                HasWinner = false,
            },
            v => JsonUtility.ToJson(v)
        );
    }

    public void SetColor(Color? newColor)
    {
        TitleBackground.color = newColor ?? defaultColor;
    }


    public ShopItem Get(RessourceType type)
    {
        switch (type)
        {
            case RessourceType.Stone: return Stone;
            case RessourceType.Wood: return Wood;
            case RessourceType.Chicken: return Chicken;
            default: return null;
        }
    }

}