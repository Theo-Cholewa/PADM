using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Victory : MonoBehaviour
{
    public List<Text> TeamName;

    public List<RawImage> Colorable;

    public static Team Winner;

    void Start()
    {
        var name = Winner?.name ?? "No One";
        var color = Winner?.color ?? Color.gray;

        foreach(var text in TeamName)
        {
            text.text = name;
        }
        
        foreach(var image in Colorable)
        {
            image.color = color;
        }
    }
}
