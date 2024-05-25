using System.Collections;
using System.Reflection;
using BoneLib;
using LabFusion.Data;
using MelonLoader;
using SLZ.Marrow.Pool;
using SLZ.Marrow.SceneStreaming;
using UnityEngine;
using Mesh = UnityEngine.Mesh;
using static Fusion5vs5Gamemode.Shared.Commons;

namespace Fusion5vs5Gamemode.Utilities;

public static class Resources
{
    public static Assembly? Fusion5vs5Assembly { get; private set; }

    private const string? BundlePath = "Fusion5vs5Gamemode.Resources.fusion5vs5custom.bundle";
    public static AssetBundle? Bundle { get; private set; }

    public const string Mp5BodyPath = "WPN_MP5";
    public static Mesh? Mp5Body { get; private set; }

    public const string PicatinnyRailMp5Path = "Picatinny Rail MP5";
    public static GameObject? PicatinnyRailMp5 { get; private set; }

    public static Material? Mk18MatBeige { get; private set; }
    public static Material? Mk18MatLightBlue { get; private set; }
    public static Material? Mk18MatDarkBlue { get; private set; }
    public static Material? Mk18MatYellow { get; private set; }

    public static GameObject? UmpBarrelGrip { get; private set; }

    private static ISpawning? Spawning { get; set; }

    public static void Initialize(ISpawning spawningInterface)
    {
        Log();

        Spawning = spawningInterface;
        if (Fusion5vs5Assembly != null) return;
        Fusion5vs5Assembly = Assembly.GetExecutingAssembly();
        Bundle = EmbeddedAssetBundle.LoadFromAssembly(Fusion5vs5Assembly, BundlePath);
        Mp5Body = Bundle.LoadPersistentAsset<Mesh>(Mp5BodyPath);
        PicatinnyRailMp5 = Bundle.LoadPersistentAsset<GameObject>(PicatinnyRailMp5Path);
        if (SceneStreamer.Session.Status != StreamStatus.DONE)
        {
            Hooking.OnLevelInitialized += HookingOnOnLevelInitialized;
        }
        else
        {
            LoadAdditionalResources();
        }
    }

    private static void HookingOnOnLevelInitialized(LevelInfo _)
    {
        Log();

        Hooking.OnLevelInitialized -= HookingOnOnLevelInitialized;
        LoadAdditionalResources();
    }

    private static void LoadAdditionalResources()
    {
        Log();

        ExtractMk18Materials();
        ExtractUmpBarrel();
    }

    private static void ExtractMk18Materials()
    {
        string[] barcodes =
        {
            // beige
            "c1534c5a-c061-4c5c-a5e2-3d955269666c",
            // light blue
            "c1534c5a-f3b6-4161-a525-a8955269666c",
            // dark blue
            "c1534c5a-5c2b-4cb4-ae31-e7955269666c",
            // yellow
            "c1534c5a-4b3e-4288-849c-ce955269666c"
        };
        foreach (var barcode in barcodes)
        {
            Spawning?.Spawn(barcode, new SerializedTransform(new Vector3(0, 0, 0), Quaternion.identity), go => { MelonCoroutines.Start(CoExtractMk18Materials(go)); });
        }
    }

    private static IEnumerator CoExtractMk18Materials(GameObject go)
    {
        AssetPoolee poolee = go.GetComponent<AssetPoolee>();
        if (poolee == null) yield break;
        string barcode = poolee.spawnableCrate.Barcode;
        Material? mat = go.transform.Find("offset_MK18/WPN_MK18")?.gameObject.GetComponent<MeshRenderer>().material;
        if (mat == null) yield break;
        mat.hideFlags = HideFlags.DontUnloadUnusedAsset;
        switch (barcode)
        {
            case "c1534c5a-c061-4c5c-a5e2-3d955269666c":
                Mk18MatBeige = mat;
                break;
            case "c1534c5a-f3b6-4161-a525-a8955269666c":
                Mk18MatLightBlue = mat;
                break;
            case "c1534c5a-5c2b-4cb4-ae31-e7955269666c":
                Mk18MatDarkBlue = mat;
                break;
            case "c1534c5a-4b3e-4288-849c-ce955269666c":
                Mk18MatYellow = mat;
                break;
        }

        Spawning?.Despawn(poolee);
    }

    private static void ExtractUmpBarrel()
    {
        Spawning?.Spawn(CommonBarcodes.Guns.MP5, new SerializedTransform(Vector3.one, Quaternion.identity), mp5 => { MelonCoroutines.Start(CoExtractUmpBarrel(mp5)); });
    }

    private static IEnumerator CoExtractUmpBarrel(GameObject mp5)
    {
        GameObject? barrelGrip = Object.Instantiate(mp5.transform.Find("BarrelGrip")?.gameObject, null, true);
        if (barrelGrip == null) yield break;
        barrelGrip.name = "fusion5vs5_BarrelGrip";
        UmpBarrelGrip = barrelGrip;
        UmpBarrelGrip.hideFlags = HideFlags.DontUnloadUnusedAsset;
        Spawning?.Despawn(mp5.GetComponent<AssetPoolee>());
    }
}
