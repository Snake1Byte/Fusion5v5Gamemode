using BoneLib;
using BoneLib.Nullables;
using Fusion5vs5Gamemode.Shared;
using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using MelonLoader;
using SLZ.Marrow.Data;
using SLZ.Marrow.Pool;
using SLZ.Marrow.Warehouse;
using UnityEngine;
using CommonBarcodes = BoneLib.CommonBarcodes;

namespace Fusion5vs5Gamemode.Utilities.DebugTools;

public static class DebugTools
{
    public const string BARCODE = CommonBarcodes.Guns.AKM;
    
    public static void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Spawnable spawnable = new Spawnable
            {
                crateRef = new SpawnableCrateReference(BARCODE)
            };
            Vector3 position = RigData.RigReferences.RigManager.physicsRig.m_pelvis.position + RigData.RigReferences.RigManager.physicsRig.m_pelvis.forward;
            Quaternion rotation = RigData.RigReferences.RigManager.physicsRig.m_pelvis.rotation;
            AssetSpawner.Register(spawnable);
            AssetSpawner.Spawn(spawnable, position, rotation, new BoxedNullable<Vector3>(null), false, new BoxedNullable<int>(null));
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            SpawnableCrateReference crateRef = new SpawnableCrateReference(BARCODE);
            Vector3 position = RigData.RigReferences.RigManager.physicsRig.m_pelvis.position + RigData.RigReferences.RigManager.physicsRig.m_pelvis.forward;
            Quaternion rotation = RigData.RigReferences.RigManager.physicsRig.m_pelvis.rotation;
            HelperMethods.SpawnCrate(crateRef, position, rotation, Vector3.one, false, null);
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            Spawnable spawnable = new Spawnable
            {
                crateRef = new SpawnableCrateReference(BARCODE)
            };
            Vector3 position = RigData.RigReferences.RigManager.physicsRig.m_pelvis.position + RigData.RigReferences.RigManager.physicsRig.m_pelvis.forward;
            Quaternion rotation = RigData.RigReferences.RigManager.physicsRig.m_pelvis.rotation;
            AssetSpawner.Register(spawnable);
            NullableMethodExtensions.PoolManager_Spawn(spawnable, position, rotation);
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            Vector3 position = RigData.RigReferences.RigManager.physicsRig.m_pelvis.position + RigData.RigReferences.RigManager.physicsRig.m_pelvis.forward;
            Quaternion rotation = RigData.RigReferences.RigManager.physicsRig.m_pelvis.rotation;
            PooleeUtilities.RequestSpawn(BARCODE, new SerializedTransform(position, rotation), PlayerIdManager.LocalId.SmallId);
        }
    }

    public static void StartGamemodeWithGame(LevelInfo level)
    {
        if (!level.barcode.Equals("Snek.csoffice.Level.Csoffice"))
        {
            MelonCoroutines.Start(Commons.RunCoRoutine(() =>
                {
                    SLZ.Marrow.SceneStreaming.SceneStreamer.Load("Snek.csoffice.Level.Csoffice");
                    LabFusion.Network.NetworkHelper.StartServer();
                }, () => true, 
                1f));
        }
        else
        {
            MelonCoroutines.Start(Commons.RunCoRoutine(() => { Client.Client.Instance?.StartGamemode(); }, () => true, 1f));
        }
    }
}
