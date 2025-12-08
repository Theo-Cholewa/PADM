using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public class Team{
    public readonly Color color;
    public readonly string name;

    public Team(Color color, string name)
    {
        this.color = color;
        this.name = name;
    }

    public static Team RED = new Team(Color.red, "red");
    public static Team BLUE = new Team(Color.blue, "blue");
    public static Team GREEN = new Team(Color.green, "green");
    public static Team YELLOW = new Team(Color.yellow, "yellow");
    public static Team PINK = new Team(new Color(1f, 0.75f, 0.8f), "pink");
    public static Team ORANGE = new Team(new Color(1f, 0.65f, 0f), "orange");
    public static Team CYAN = new Team(new Color(0f, 1f, 1f), "cyan");
    public static Team MAGENTA = new Team(new Color(1f, 0f, 1f), "magenta");

    public static Team[] AllTeams = new Team[] {
        RED, BLUE, GREEN,
        YELLOW, PINK, ORANGE,
        CYAN, MAGENTA
    };

    public enum TeamEnum
    {
        RED, BLUE, GREEN,
        YELLOW, PINK, ORANGE,
        CYAN, MAGENTA
    }

    private static Team _currentTeam = RED;
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
