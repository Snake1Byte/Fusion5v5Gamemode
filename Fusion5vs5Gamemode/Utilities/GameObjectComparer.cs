using System.Collections.Generic;
using UnityEngine;

namespace Fusion5vs5Gamemode.Utilities;

public class GameObjectComparer : IEqualityComparer<GameObject>
{
    public bool Equals(GameObject x, GameObject y)
    {
        return x.GetInstanceID() == y.GetInstanceID();
    }

    public int GetHashCode(GameObject obj)
    {
        return obj.GetHashCode();
    }
}
