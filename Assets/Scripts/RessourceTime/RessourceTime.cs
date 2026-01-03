using UnityEngine;

public class RessourceTime : MonoBehaviour
{
    void FixedUpdate()
    {
        RessourceClient.current.GoToGoodScene();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.B))
        {
            Debug.Log("ASk for fight");
            RessourceClient.current.Get(Team.RED).AskForFight();
        }
    }
}
