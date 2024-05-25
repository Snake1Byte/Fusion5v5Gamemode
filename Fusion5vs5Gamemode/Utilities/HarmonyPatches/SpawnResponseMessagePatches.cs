using System;
using HarmonyLib;
using LabFusion.Network;
using UnityEngine;

namespace  Fusion5vs5Gamemode.Utilities.HarmonyPatches;

[HarmonyPatch(typeof(SpawnResponseMessage))]
public static class SpawnResponseMessagePatches
{
    public static Action<byte, string, ushort, GameObject>? OnSpawnFinished;
        
    [HarmonyPostfix]
    [HarmonyPatch(nameof(SpawnResponseMessage.OnSpawnFinished))]
    public static void SpawnFinished(byte owner, string barcode, ushort syncId, GameObject go)
    {
        SafeActions.InvokeActionSafe(OnSpawnFinished, owner, barcode, syncId, go);
    }
}