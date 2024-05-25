using System.Collections.Generic;
using System.Linq;
using BoneLib;
using HarmonyLib;
using LabFusion.Data;
using MelonLoader;
using SLZ.Interaction;
using UnityEngine;
using Resources = Fusion5vs5Gamemode.Utilities.Resources;
using static Fusion5vs5Gamemode.Shared.Commons;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Fusion5vs5Gamemode.Client.Combat;

public static class AssetDatabase
{
    public static Dictionary<string, Attachments>? AttachmentDatabase { get; private set; }

    public static void InitializeDatabase()
    {
        Log();

        AttachmentDatabase = new();
        string barcode = CommonBarcodes.Guns.MK18Naked;
        Attachments attachments = new Attachments();
        attachments.PicatinnySlotsToAdd.Add("Top 1", new SerializedTransform(new Vector3(0f, 0.0611f, -0.0281f), new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 2", new SerializedTransform(new Vector3(0f, 0.0611f, 0.05459976f), new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 3", new SerializedTransform(new Vector3(0f, 0.0611f, 0.137599f), new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 4", new SerializedTransform(new Vector3(0f, 0.0611f, 0.2203f), new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 5", new SerializedTransform(new Vector3(0f, 0.0611f, 0.3031f), new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Left 1", new SerializedTransform(new Vector3(-0.0248f, 0.0321f, 0.30788f), new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Right 1", new SerializedTransform(new Vector3(0.02475f, 0.03243f, 0.30788f), new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 1", new SerializedTransform(new Vector3(0f, 0.003900051f, 0.1338f), new Quaternion(0f, 0f, 1f, -3.576278E-07f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 2", new SerializedTransform(new Vector3(0f, 0.003900051f, 0.2168007f), new Quaternion(0f, 0f, 1f, -3.576278E-07f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 3", new SerializedTransform(new Vector3(0f, 0.003900051f, 0.2994995f), new Quaternion(0f, 0f, 1f, -3.576278E-07f)));
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightRear", "Top 1"); // TODO new dependency :hollow:
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightFront", "Top 5");
        attachments.CustomActionUponSpawn += Mk18RandomizeColor;
        AttachmentDatabase.Add(barcode, attachments);

        barcode = CommonBarcodes.Guns.PDRC;
        attachments = new Attachments();
        attachments.GameObjectsToRemove.Add("LaserPointer_01");
        attachments.PicatinnySlotsToAdd.Add("Top 1", new SerializedTransform(new Vector3(0f, 0.0494f, 0.0051f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 2", new SerializedTransform(new Vector3(0f, 0.0494f, 0.0668f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 3", new SerializedTransform(new Vector3(0f, 0.0494f, 0.1363f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 4", new SerializedTransform(new Vector3(0f, 0.0494f, 0.2004f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightRear", "Top 1");
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightFront", "Top 4");
        AttachmentDatabase.Add(barcode, attachments);

        barcode = CommonBarcodes.Guns.M16IronSights;
        attachments = new Attachments();
        attachments.GameObjectsToRemove.Add("WPN_M16_Exchangeables_frontsight");
        attachments.GameObjectsToRemove.Add("Attachment_FoldingRearSight (1)");
        attachments.GameObjectsToRemove.Add("WPN_M16_Receiver.001");
        attachments.PicatinnySlotsToAdd.Add("Top 1", new SerializedTransform(new Vector3(0f, 0.0613f, -0.0221f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 2", new SerializedTransform(new Vector3(0f, 0.0613f, 0.0561f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 3", new SerializedTransform(new Vector3(0f, 0.0613f, 0.1465f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 4", new SerializedTransform(new Vector3(0f, 0.0613f, 0.235f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 5", new SerializedTransform(new Vector3(0f, 0.0613f, 0.3202f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 6", new SerializedTransform(new Vector3(0f, 0.0613f, 0.4059f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Left 1", new SerializedTransform(new Vector3(-0.0286f, 0.0324f, 0.1461996f), new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Left 2", new SerializedTransform(new Vector3(-0.0286f, 0.0324f, 0.2144f), new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Left 3", new SerializedTransform(new Vector3(-0.0286f, 0.0324f, 0.2835f), new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Left 4", new SerializedTransform(new Vector3(-0.0286f, 0.0324f, 0.3492f), new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Right 1", new SerializedTransform(new Vector3(0.0284f, 0.0324f, 0.1461996f), new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Right 2", new SerializedTransform(new Vector3(0.0284f, 0.0324f, 0.2144f), new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Right 3", new SerializedTransform(new Vector3(0.0284f, 0.0324f, 0.2835f), new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Right 4", new SerializedTransform(new Vector3(0.0284f, 0.0324f, 0.3492f), new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 1", new SerializedTransform(new Vector3(0f, 0.004300003f, 0.1465f), new Quaternion(0f, 0f, 1f, 0f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 2", new SerializedTransform(new Vector3(0f, 0.004300003f, 0.235f), new Quaternion(0f, 0f, 1f, 0f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 3", new SerializedTransform(new Vector3(0f, 0.004300003f, 0.3202f), new Quaternion(0f, 0f, 1f, 0f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 4", new SerializedTransform(new Vector3(0f, 0.0043f, 0.4059f), new Quaternion(0f, 0f, 1f, 0f)));
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightRear", "Top 1");
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightFront", "Top 6");
        AttachmentDatabase.Add(barcode, attachments);

        barcode = CommonBarcodes.Guns.FAB;
        attachments = new Attachments();
        attachments.GameObjectsToRemove.Add("attachment_clamp");
        attachments.GameObjectsToRemove.Add("attachment_Lazer");
        attachments.PicatinnySlotsToAdd.Add("Top 1", new SerializedTransform(new Vector3(0f, 0.07173f, 0.05980012f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 2", new SerializedTransform(new Vector3(0f, 0.07173f, 0.1212f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 3", new SerializedTransform(new Vector3(0f, 0.07173f, 0.18597f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Right 1", new SerializedTransform(new Vector3(0.02402f, 0.0511f, 0.54196f), new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.CustomActionUponSpawn += o => o.transform.Find("FDefenceOne(Dmg)")?.gameObject.SetActive(true);
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightRear", "Top 1");
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightFront", "Top 3");
        AttachmentDatabase.Add(barcode, attachments);

        barcode = CommonBarcodes.Guns.M4;
        attachments = new Attachments();
        attachments.PicatinnySlotsToAdd.Add("Top 1", new SerializedTransform(new Vector3(0f, 0.0525f, 0.0397f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 2", new SerializedTransform(new Vector3(0f, 0.0525f, 0.0875f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 3", new SerializedTransform(new Vector3(0f, 0.0525f, 0.1346f), new Quaternion(0f, 0f, 0f, 1f)));
        AttachmentDatabase.Add(barcode, attachments);

        barcode = CommonBarcodes.Guns.M590A1;
        attachments = new Attachments();
        attachments.GameObjectsToRemove.Add("attachment_HoloScope");
        attachments.PicatinnySlotsToAdd.Add("Top 1", new SerializedTransform(new Vector3(0f, 0.06682f, 0.01500008f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 2", new SerializedTransform(new Vector3(0f, 0.06682f, 0.0552f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 3", new SerializedTransform(new Vector3(0f, 0.06682f, 0.1002f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightRear", "Top 1");
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightFront", "Top 3");
        AttachmentDatabase.Add(barcode, attachments);

        barcode = CommonBarcodes.Guns.UMP;
        attachments = new Attachments();
        attachments.GameObjectsToRemove.Add("attachment_ForwardGrip");
        attachments.PicatinnySlotsToAdd.Add("Top 1", new SerializedTransform(new Vector3(0f, 0.0701f, -0.0489f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 2", new SerializedTransform(new Vector3(0f, 0.0701f, 0.0001000036f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 3", new SerializedTransform(new Vector3(0f, 0.0701f, 0.0499f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Left 1", new SerializedTransform(new Vector3(-0.025f, 0.0199f, 0.15745f), new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Left 2", new SerializedTransform(new Vector3(-0.025f, 0.0199f, 0.18894f), new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Left 3", new SerializedTransform(new Vector3(-0.025f, 0.0199f, 0.21869f), new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Right 1", new SerializedTransform(new Vector3(0.02480002f, 0.0199f, 0.1562f), new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Right 2", new SerializedTransform(new Vector3(0.02480002f, 0.0199f, 0.18769f), new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Right 3", new SerializedTransform(new Vector3(0.0248f, 0.0199f, 0.21744f), new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 1", new SerializedTransform(new Vector3(0f, -0.00691f, 0.15796f), new Quaternion(0f, 0f, 1f, 0f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 2", new SerializedTransform(new Vector3(0f, -0.00691f, 0.18945f), new Quaternion(0f, 0f, 1f, 0f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 3", new SerializedTransform(new Vector3(0f, -0.00691f, 0.2192f), new Quaternion(0f, 0f, 1f, 0f)));
        attachments.CustomActionUponSpawn += UmpAddBarrelGrip;
        AttachmentDatabase.Add(barcode, attachments);

        barcode = CommonBarcodes.Guns.MP5;
        attachments = new Attachments();
        attachments.PicatinnySlotsToAdd.Add("Top 1", new SerializedTransform(new Vector3(0f, 0.08788f, -0.11489f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 2", new SerializedTransform(new Vector3(0f, 0.08788f, -0.07570003f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 3", new SerializedTransform(new Vector3(0f, 0.08788f, -0.03658003f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 4", new SerializedTransform(new Vector3(0f, 0.08788f, 0.002609983f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 5", new SerializedTransform(new Vector3(0f, 0.08788f, 0.0413f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 6", new SerializedTransform(new Vector3(0f, 0.08788f, 0.08049001f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightRear", "Top 1");
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightFront", "Top 6");
        attachments.CustomActionUponSpawn += Mp5AddCustomBody;
        AttachmentDatabase.Add(barcode, attachments);

        barcode = CommonBarcodes.Guns.Vector;
        attachments = new Attachments();
        attachments.GameObjectsToRemove.Add("offset/attachment_HoloScope");
        attachments.PicatinnySlotsToAdd.Add("Top 1", new SerializedTransform(new Vector3(0f, 0.10319f, -0.0845f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 2", new SerializedTransform(new Vector3(0f, 0.10319f, -0.04095f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 3", new SerializedTransform(new Vector3(0f, 0.10319f, 0.0026f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 4", new SerializedTransform(new Vector3(0f, 0.10319f, 0.0471f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 5", new SerializedTransform(new Vector3(0f, 0.10319f, 0.09064999f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 6", new SerializedTransform(new Vector3(0f, 0.10319f, 0.1342f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 7", new SerializedTransform(new Vector3(0f, 0.10319f, 0.1784f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Left 1", new SerializedTransform(new Vector3(-0.02034f, 0.0456f, 0.1529f), new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Left 2", new SerializedTransform(new Vector3(-0.02034f, 0.0456f, 0.19645f), new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Right 1", new SerializedTransform(new Vector3(0.0201f, 0.0453f, 0.15375f), new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Right 2", new SerializedTransform(new Vector3(0.0201f, 0.0453f, 0.1973f), new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 1", new SerializedTransform(new Vector3(0f, 0.0243f, 0.1548f), new Quaternion(0f, 0f, 1f, 0f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 2", new SerializedTransform(new Vector3(0f, 0.0243f, 0.19835f), new Quaternion(0f, 0f, 1f, 0f)));
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightRear", "Top 1");
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightFront", "Top 7");
        AttachmentDatabase.Add(barcode, attachments);

        barcode = CommonBarcodes.Guns.UZI;
        attachments = new Attachments();
        attachments.PicatinnySlotsToAdd.Add("Top 1", new SerializedTransform(new Vector3(0f, 0.0461f, -0.0645f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 2", new SerializedTransform(new Vector3(0f, 0.0461f, -0.0236f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 3", new SerializedTransform(new Vector3(0f, 0.0461f, 0.0197f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 1", new SerializedTransform(new Vector3(0f, -0.0296f, 0.0599f), new Quaternion(0f, 0f, 1f, 0f)));
        AttachmentDatabase.Add(barcode, attachments);

        barcode = CommonBarcodes.Guns.P350;
        attachments = new Attachments();
        attachments.PicatinnySlotsToAdd.Add("Bottom 1", new SerializedTransform(new Vector3(0f, -0.01695013f, 0.0654f), new Quaternion(0f, 0f, 1f, 0f)));
        AttachmentDatabase.Add(barcode, attachments);

        barcode = CommonBarcodes.Guns.Eder22;
        attachments = new Attachments();
        attachments.PicatinnySlotsToAdd.Add("Bottom 1", new SerializedTransform(new Vector3(0f, -0.01440001f, 0.0849f), new Quaternion(0f, 0f, 1f, 0f)));
        AttachmentDatabase.Add(barcode, attachments);
    }

    private static void Mk18RandomizeColor(GameObject mk18)
    {
        Log(mk18);

        Material? mat = null;
        switch (Random.RandomRangeInt(0, 4))
        {
            case 0:
                mat = Resources.Mk18MatBeige;
                break;
            case 1:
                mat = Resources.Mk18MatYellow;
                break;
            case 2:
                mat = Resources.Mk18MatDarkBlue;
                break;
            case 3:
                mat = Resources.Mk18MatLightBlue;
                break;
        }

        if (mat == null) return;
        GameObject? mk18Meshes = mk18.transform.Find("offset_MK18/WPN_MK18")?.gameObject;
        if (mk18Meshes == null) return;
        MeshRenderer[] renderers = mk18Meshes.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.material = mat;
        }
    }

    private static void UmpAddBarrelGrip(GameObject ump)
    {
        Log(ump);

        // Fix dynamically grip
        GameObject? barrelGrip = Object.Instantiate(Resources.UmpBarrelGrip, ump.transform, false);
        if (barrelGrip == null) return;
        barrelGrip.transform.localPosition = new UnityEngine.Vector3(0, 0, 0);
        barrelGrip.transform.rotation = UnityEngine.Quaternion.identity;
        barrelGrip.hideFlags = HideFlags.None;
        
        CylinderGrip grip = barrelGrip.GetComponent<CylinderGrip>();
        grip.attachedHandDelegate = null;
        grip.detachedHandDelegate = null;
        InteractableHost host = ump.GetComponent<InteractableHost>();
        host._grips.Add(grip);
        grip.Host = host.TryCast<IGrippable>();

        InteractableIcon? magGripInteractableIcon = ump.transform.Find("MagGrip")?.GetComponent<InteractableIcon>();
        if (magGripInteractableIcon == null) return;
        InteractableIcon interactableIcon = barrelGrip.GetComponent<InteractableIcon>();
        interactableIcon.Mblock = magGripInteractableIcon.Mblock;
        interactableIcon.RemoveIcon();
        interactableIcon.AddIcon();

        MelonCoroutines.Start(CoRunUponCondition(() =>
            {
                barrelGrip.transform.localPosition = new UnityEngine.Vector3(0.0002f, 0.0204f, 0.18f);
                barrelGrip.transform.localRotation = UnityEngine.Quaternion.identity;
            }, () => ump.transform.parent == null
        ));
    }

    private static void Mp5AddCustomBody(GameObject mp5)
    {
        Log(mp5);

        MeshFilter? filter = mp5.transform.Find("offset_MP5/WPN_MP5")?.gameObject.GetComponent<MeshFilter>();
        if (filter == null) return;
        Mesh? mp5Body = Resources.Mp5Body;
        if (mp5Body == null) return;
        filter.mesh = mp5Body;
        filter = mp5.transform.Find("offset_MP5/WPN_MP5/005_static_WPN_MP5")?.gameObject.GetComponent<MeshFilter>();
        if (filter == null) return;
        filter.mesh = mp5Body;
        GameObject? picatinnyRailMp5 = Resources.PicatinnyRailMp5;
        if (picatinnyRailMp5 == null) return;
        GameObject? offsetMp5 = mp5.transform.Find("offset_MP5")?.gameObject;
        if (offsetMp5 == null) return;
        picatinnyRailMp5 = Object.Instantiate(picatinnyRailMp5, offsetMp5.transform, true);
        picatinnyRailMp5.transform.localPosition = new UnityEngine.Vector3(0, 0.0662534f, 0.05875346f);
        picatinnyRailMp5.transform.localEulerAngles = new UnityEngine.Vector3(-90, 0, 0);
        Collider[] cols = picatinnyRailMp5.GetComponentsInChildren<Collider>().ToArray();
        foreach (var col in cols)
        {
            mp5.GetComponent<InteractableHost>()?.Colliders.AddItem(col);
        }
    }
}
