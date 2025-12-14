using UnityEngine;

public class TeamFlag : MonoBehaviour
{
    public MeshRenderer Colored;

    [SerializeField]
    public Team team;

    public string targetScene;

    void Start()
    {
        Colored.material.color = team.color;
    }

    void OnTouchDown(TouchInfo info)
    {
        if(targetScene.Length>0) UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
    }

}
