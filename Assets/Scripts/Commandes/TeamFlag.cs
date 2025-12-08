using UnityEngine;

public class TeamFlag : MonoBehaviour
{
    public MeshRenderer Colored;

    [SerializeField]
    public Team.TeamEnum team;

    public string targetScene;

    void Start()
    {
        var team = Team.AllTeams[(int)this.team];
        Colored.material.color = team.color;
    }

    void OnTouchDown(TouchInfo info)
    {
        Team.currentTeam = Team.AllTeams[(int)team];
        if(targetScene.Length>0) UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
    }

}
