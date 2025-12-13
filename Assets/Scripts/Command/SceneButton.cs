using UnityEngine;

public class SceneButton : MonoBehaviour
{
    public MeshRenderer Background;
    public string targetScene;

    bool isPressed = false;

    void OnTouchDown(TouchInfo info)
    {
        Background.transform.localScale = Vector3.one * 1.2f;
        isPressed = true;
    }


    void OnTouchUp(TouchInfo info)
    {
        if (isPressed)
        {
            if(targetScene.Length>0) UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
        }
        isPressed = false;
    }

    void OnTouchDragEnd(TouchInfo info)
    {
        Background.transform.localScale = Vector3.one;
        isPressed = false;
    }

}
