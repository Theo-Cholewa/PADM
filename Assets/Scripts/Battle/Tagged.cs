using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tagged : MonoBehaviour
{
    public List<string> Tag;

    public static bool Is(GameObject obj, string tag)
    {
        return obj.TryGetComponent<Tagged>(out var tagged) && tagged.Tag.Contains(tag);
    }
}
