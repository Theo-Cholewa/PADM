using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Tappable : MonoBehaviour
{
    public List<Material> materials;
    public MeshRenderer renderer;
    public AudioClip tapSound;

    public int tapCount = 3;

    private int currentCount = 0;

    void OnTouchDown(TouchInfo info)
    {
        currentCount++;
        GetComponent<AudioSource>().PlayOneShot(tapSound);
        var currentMat = currentCount / tapCount;
        if (currentMat < materials.Count)
        {
            renderer.material = materials[currentMat];
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
