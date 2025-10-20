using System.Collections.Generic;
using UnityEngine;

public class Tagged : MonoBehaviour
{
    public List<Tag> Tags;

    public static bool Is(GameObject obj, Tag tag)
    {
        var tagged = obj.GetComponent<Tagged>();
        return tagged != null && tagged.Tags.Contains(tag);
    }

    public enum Tag
    {
        
    }
}