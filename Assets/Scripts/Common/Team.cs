
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[Serializable]
public enum TeamEnum
{
    RED, BLUE, GREEN,
    YELLOW, PINK, ORANGE,
    CYAN, MAGENTA
}


public class Team{
    public readonly Color color;
    public readonly string id;
    public readonly string name;
    public readonly TeamEnum enumValue;

    private static Dictionary<string,Team> STRING_TO_TEAM = new Dictionary<string, Team>();

    private Team(Color color, string id, string name, TeamEnum enumv)
    {
        this.color = color;
        this.id = id;
        this.name = name;
        this.enumValue = enumv;
        STRING_TO_TEAM.Add(id,this);
    }

    public static Team RED = new Team(Color.red, "red", "Red", TeamEnum.RED);
    public static Team BLUE = new Team(Color.blue, "blue", "Blue", TeamEnum.BLUE);
    public static Team GREEN = new Team(Color.green, "green", "Green", TeamEnum.GREEN);
    public static Team YELLOW = new Team(Color.yellow, "yellow", "Yellow", TeamEnum.YELLOW);
    public static Team PINK = new Team(new Color(1f, 0.75f, 0.8f), "pink", "Pink", TeamEnum.PINK);
    public static Team ORANGE = new Team(new Color(1f, 0.65f, 0f), "orange", "Orange", TeamEnum.ORANGE);
    public static Team CYAN = new Team(new Color(0f, 1f, 1f), "cyan", "Cyan", TeamEnum.CYAN);
    public static Team MAGENTA = new Team(new Color(1f, 0f, 1f), "magenta", "Magenta", TeamEnum.MAGENTA);


    private static Team[] ENUM_TO_TEAM = new Team[]
    {
        RED, BLUE, GREEN,
        YELLOW, PINK, ORANGE,
        CYAN, MAGENTA
    };

    public static Team Of(TeamEnum team)
    {
        return ENUM_TO_TEAM[(int)team];
    }


    public static Team Parse(string name)
    {
        return STRING_TO_TEAM[name];
    }

    public string Serialize()
    {
        return id;
    }

    public override string ToString()
    {
        return name;
    }

    private static Team _currentTeam = Team.RED;
    public static Team currentTeam
    {
        get => _currentTeam;
        set {
            _currentTeam = value;
            onTeamChanged.Invoke(value);
        }
    }
    
    public static UnityEvent<Team> onTeamChanged = new();
}