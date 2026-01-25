using UnityEngine;

public static class General
{

    public static void Reset(){
        UnityEngine.SceneManagement.SceneManager.LoadScene("TeamSelection");
        Object.Destroy(RessourceClient.current.gameObject);
    }

}
