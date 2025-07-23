using System.Collections.Generic;
using UnityEngine;

// Why we are using this class ?
// The skeleton only generate points, not prefabs.
// Therefore this class will read all the child gameobject placed in it
// Then it will init these game object based on the world space (it has no relation to the skeleton points).
// The decors will be placed inside the skeleton chunk so provide a parent for it at runtime
public class TerrainPrefabDecorController : MonoBehaviour
{
    public List<Transform> ExtractDecorations(Transform parentTransform)
    {
        var children = new List<Transform>();
        foreach (Transform child in parentTransform)
        {
            children.Add(child);
        }
        return children;
    }

    public bool IsThisPrefabHasChildren(Transform parent)
    {
        return parent.childCount > 0;
    }

}
