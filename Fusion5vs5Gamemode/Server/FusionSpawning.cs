using System;
using System.Collections.Generic;
using BoneLib;
using Fusion5vs5Gamemode.Client;
using Fusion5vs5Gamemode.Shared.Modules;
using Fusion5vs5Gamemode.Utilities;
using Fusion5vs5Gamemode.Utilities.HarmonyPatches;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Syncables;
using LabFusion.Utilities;
using SLZ.Marrow.Pool;
using UnityEngine;
using static Fusion5vs5Gamemode.Shared.Commons;
using SafeActions = Fusion5vs5Gamemode.Utilities.SafeActions;

namespace Fusion5vs5Gamemode.Server;

public class FusionSpawning : ISpawning
{
    public byte Owner { get; }

    private struct SpawnedObject
    {
        public string Barcode;
        public byte Owner;
        public Action<byte, string, ushort, GameObject> SpawnAction;
    }

    private static readonly List<SpawnedObject> SpawnQueue = new();
    private static readonly List<SpawnedObject> DeferredServerSpawnQueue = new();

    public FusionSpawning(byte owner)
    {
        Log(owner);
        
        Owner = owner;
    }

    static FusionSpawning()
    {
        Log();
        
        ModuleMessages.DeferredItemSpawned += DeferredItemSpawned;
        Hooking.OnLevelInitialized += EmptyLists;
    }

    private static void EmptyLists(LevelInfo obj)
    {
        Log(obj);
        
        lock (SpawnQueue)
        {
            SpawnQueue.Clear();
        }

        lock (DeferredServerSpawnQueue)
        {
            DeferredServerSpawnQueue.Clear();
        }
    }

    public static void RequestSpawn(string barcode, SerializedTransform transform, byte owner, Action<byte, string, ushort, GameObject>? onSpawn)
    {
        Log(barcode, transform, owner, onSpawn!);
        if (onSpawn != null) AddToSpawnQueue(barcode, owner, onSpawn);
        PooleeUtilities.RequestSpawn(barcode, transform, owner);
    }

    private static void OnSpawnFinished(byte owner, string barcode, ushort syncId, GameObject go)
    {
        Log(owner, barcode, syncId, go);

        if (!TryFindInSpawnQueue(owner, barcode, out SpawnedObject? obj)) return;
        Action<byte, string, ushort, GameObject> onSpawn = obj!.Value.SpawnAction;
        RemoveFromSpawnQueue(obj.Value);
        SafeActions.InvokeActionSafe(onSpawn, owner, barcode, syncId, go);
    }

    private static void AddToSpawnQueue(string barcode, byte owner, Action<byte, string, ushort, GameObject> onSpawn)
    {
        Log(barcode, owner, onSpawn);
        
        lock (SpawnQueue)
        {
            if (SpawnQueue.Count == 0)
            {
                SpawnResponseMessagePatches.OnSpawnFinished += OnSpawnFinished;
            }

            SpawnQueue.Add(new SpawnedObject { Barcode = barcode, Owner = owner, SpawnAction = onSpawn });
        }
    }

    private static bool TryFindInSpawnQueue(byte owner, string barcode, out SpawnedObject? spawnedObject)
    {
        Log(owner, barcode, null!);
        
        lock (SpawnQueue)
        {
            foreach (SpawnedObject obj in SpawnQueue)
            {
                if (obj.Barcode.Equals(barcode) && obj.Owner.Equals(owner))
                {
                    spawnedObject = obj;
                    return true;
                }
            }
        }

        spawnedObject = null;
        return false;
    }

    private static void RemoveFromSpawnQueue(SpawnedObject toRemove)
    {
        Log(toRemove);
        
        lock (SpawnQueue)
        {
            SpawnQueue.Remove(toRemove);
            if (SpawnQueue.Count == 0)
            {
                SpawnResponseMessagePatches.OnSpawnFinished -= OnSpawnFinished;
            }
        }
    }

    private static void DeferredItemSpawned(ushort syncId, byte owner, string barcode)
    {
        Log(syncId, owner, barcode);

        if (!TryFindInDeferredServerSpawnQueue(owner, barcode, out SpawnedObject? obj)) return;
        Action<byte, string, ushort, GameObject> onSpawn = obj!.Value.SpawnAction;
        RemoveFromDeferredServerSpawnQueue(obj.Value);
        if (!SyncManager.TryGetSyncable(syncId, out PropSyncable syncable)) return;
        SafeActions.InvokeActionSafe(onSpawn, owner, barcode, syncId, syncable.GameObject);
    }

    private static void AddToDeferredServerSpawnQueue(byte owner, string barcode, Action<byte, string, ushort, GameObject> onSpawn)
    {
        Log(owner, barcode, onSpawn);
        
        lock (DeferredServerSpawnQueue)
        {
            DeferredServerSpawnQueue.Add(new SpawnedObject { Barcode = barcode, Owner = owner, SpawnAction = onSpawn });
        }
    }

    private static bool TryFindInDeferredServerSpawnQueue(byte owner, string barcode, out SpawnedObject? spawnedObject)
    {
        Log(owner, barcode, null!);
        
        lock (DeferredServerSpawnQueue)
        {
            foreach (SpawnedObject obj in DeferredServerSpawnQueue)
            {
                if (obj.Barcode.Equals(barcode) && obj.Owner.Equals(owner))
                {
                    spawnedObject = obj;
                    return true;
                }
            }
        }

        spawnedObject = null;
        return false;
    }

    private static void RemoveFromDeferredServerSpawnQueue(SpawnedObject toRemove)
    {
        Log(toRemove);
        
        lock (DeferredServerSpawnQueue)
        {
            DeferredServerSpawnQueue.Remove(toRemove);
        }
    }

    // Needs to get run on every client simultaneously to work
    public static void DeferredServerSpawn(string barcode, byte owner, SerializedTransform transform, Action<byte, string, ushort, GameObject>? onSpawn = null)
    {
        Log(barcode, owner, transform, onSpawn!);
        
        if (onSpawn != null)
        {
            AddToDeferredServerSpawnQueue(owner, barcode, onSpawn);
        }

        if (NetworkInfo.IsServer)
        {
            RequestSpawn(barcode, transform, owner, (ownerInner, barcodeInner, syncId, _) => { ModuleRequests.BroadcastDeferredItemSpawned(syncId, ownerInner, barcodeInner); });
        }
    }

    private static void DeferredServerDespawn(AssetPoolee poolee)
    {
        Log(poolee);
        
        if (!NetworkInfo.IsServer || poolee == null) return;
        GameObject go = poolee.gameObject;
        foreach (ISyncable syncable in SyncManager.Syncables.Values)
        {
            if (syncable is PropSyncable propSyncable && propSyncable.GameObject.GetInstanceID() == go.GetInstanceID())
            {
                PooleeUtilities.RequestDespawn(syncable.GetId());
            }
        }
    }

    public void Spawn(string barcode, SerializedTransform transform, Action<GameObject> onSpawn)
    {
        Log(barcode, transform, onSpawn);
        
        DeferredServerSpawn(barcode, Owner, transform, (_, _, _, gameObject) => onSpawn(gameObject));
    }

    public void Despawn(AssetPoolee poolee)
    {
        Log(poolee);

        DeferredServerDespawn(poolee);
    }
}
