using UnityEngine;

public class TeamFlag : MonoBehaviour
{
    public MeshRenderer Colored;

    public TeamEnum TeamId;

    public Team team => Team.Of(TeamId);

    public string targetScene;

    void Start()
    {
        Colored.material.color = team.color;
    }

    void OnTouchDown(TouchInfo info)
    {
        if (targetScene.Length > 0)
        {
            Team.currentTeam = team;
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
        }
    }

}
