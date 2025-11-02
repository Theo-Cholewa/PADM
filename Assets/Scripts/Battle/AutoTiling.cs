using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTiling : MonoBehaviour
{
    public Renderer renderer;

    void Start()
    {
        AutoTile();
    }

    void Update()
    {

    }

    void OnTransformParentChanged()
    {
        AutoTile();
    }

    void AutoTile()
    {
        var material = renderer.material;
        var transform = renderer.transform;

        var ratio = transform.lossyScale.x / transform.lossyScale.z;
        var textureRatio = material.mainTexture.width / material.mainTexture.height;

        renderer.material.mainTextureScale = new Vector2(ratio/textureRatio, 1);
    }
}
