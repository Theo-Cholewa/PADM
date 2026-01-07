using System.Collections.Generic;


public static class RessourceTypes
{
    public static HashSet<RessourceType> UPGRADES = new HashSet<RessourceType>
    {
        RessourceType.Cannon, RessourceType.Pirate, RessourceType.Barrel, RessourceType.Ship
    };

    public static HashSet<RessourceType> RESSOURCES = new HashSet<RessourceType>
    {
        RessourceType.Wood, RessourceType.Stone, RessourceType.Chicken
    };
}

public enum RessourceType
{
    Wood,
    Stone,
    Chicken,

    Cannon,
    Pirate,
    Barrel,
    Ship,

    Gold,
    Health
}

