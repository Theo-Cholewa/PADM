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

    public int Get(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Gold: return gold;

            case ResourceType.Wood: return wood;
            case ResourceType.Rock: return rock;
            case ResourceType.Chicken: return chicken;

            case ResourceType.Cannon: return cannonLevel;
            case ResourceType.Pirate: return pirateLevel;
            case ResourceType.Barrel: return barrelLevel;
            case ResourceType.Ship: return shipLevel;

            default: return 0;
        }
    }

    public void Set(ResourceType type, int value)
    {
        switch (type)
        {
            case ResourceType.Gold: gold = value; break;

            case ResourceType.Wood: wood = value; break;
            case ResourceType.Rock: rock = value; break;
            case ResourceType.Chicken: chicken = value; break;

            case ResourceType.Cannon: cannonLevel = value; break;
            case ResourceType.Pirate: pirateLevel = value; break;
            case ResourceType.Barrel: barrelLevel = value; break;
            case ResourceType.Ship: shipLevel = value; break;

            default: break;
        }
    }
}