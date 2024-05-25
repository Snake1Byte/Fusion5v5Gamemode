using System;
using LabFusion.Data;
using SLZ.Marrow.Pool;
using UnityEngine;

namespace Fusion5vs5Gamemode.Utilities;

public interface ISpawning
{
    void Spawn(string barcode, SerializedTransform transform, Action<GameObject> onSpawn);

    void Despawn(AssetPoolee poolee);
}
