using System;

[Serializable]
public struct RessourceData
{
    public int gold;
    public int wood;
    public int rock;
    public int chicken;
    public int cannonLevel;
    public int pirateLevel;
    public int barrelLevel;
    public int shipLevel;
    public int health;

    public int Get(RessourceType type)
    {
        switch (type)
        {
            case RessourceType.Gold: return gold;
            case RessourceType.Health: return health;

            case RessourceType.Wood: return wood;
            case RessourceType.Rock: return rock;
            case RessourceType.Chicken: return chicken;

            case RessourceType.Cannon: return cannonLevel;
            case RessourceType.Pirate: return pirateLevel;
            case RessourceType.Barrel: return barrelLevel;
            case RessourceType.Ship: return shipLevel;

            default: return 0;
        }
    }

    public void Set(RessourceType type, int value)
    {
        switch (type)
        {
            case RessourceType.Gold: gold = value; break;
            case RessourceType.Health: health = value; break;

            case RessourceType.Wood: wood = value; break;
            case RessourceType.Rock: rock = value; break;
            case RessourceType.Chicken: chicken = value; break;

            case RessourceType.Cannon: cannonLevel = value; break;
            case RessourceType.Pirate: pirateLevel = value; break;
            case RessourceType.Barrel: barrelLevel = value; break;
            case RessourceType.Ship: shipLevel = value; break;

            default: break;
        }
    }
}