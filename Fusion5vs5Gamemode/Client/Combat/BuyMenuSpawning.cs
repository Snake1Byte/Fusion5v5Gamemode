using System.Collections;
using System.Collections.Generic;
using BoneLib;
using Fusion5vs5Gamemode.Server;
using Fusion5vs5Gamemode.Utilities;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using MelonLoader;
using SLZ.Interaction;
using SLZ.Props.Weapons;
using SLZ.Rig;
using SLZ.SFX;
using UnityEngine;
using static Fusion5vs5Gamemode.Shared.Commons;

namespace Fusion5vs5Gamemode.Client.Combat;

public class BuyMenuSpawning
{
    private static readonly Dictionary<GameObject, List<Renderer>> _RenderersHidden = new(new GameObjectComparer());

    static BuyMenuSpawning()
    {
        Hooking.OnLevelInitialized += EmptyLists;
    }

    private static void EmptyLists(LevelInfo _)
    {
        _RenderersHidden.Clear();
    }
    
    private static void PlaceInPlayerInventory(GameObject gameObject, PlayerId player)
    {
        Log(gameObject, player);
        try
        {
            WeaponSlot? weaponSlot = gameObject.GetComponentInChildren<WeaponSlot>();
            InteractableHost? host = gameObject.GetComponentInChildren<InteractableHost>();
            if (weaponSlot == null || host == null) return;

            RigReferenceCollection? rigReferences = GetRigReferences(player);
            if (rigReferences == null) return;
            foreach (var slot in rigReferences.RigSlots)
            {
                if (slot._slottedWeapon == null && (slot.slotType & weaponSlot.slotType) != 0)
                {
                    slot.InsertInSlot(host);
                    rigReferences.RightHand.gameObject.GetComponent<HandSFX>()?.BodySlot();
                    return;
                }
            }
        }
        finally
        {
            if (_RenderersHidden.TryGetValue(gameObject, out List<Renderer> renderers))
            {
                _RenderersHidden.Remove(gameObject);
                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = true;
                }
            }
        }
    }

    public static void PlayerBoughtItem(PlayerId player, string barcode)
    {
        Log(player, barcode);

        RigReferenceCollection? rigReferences = GetRigReferences(player);
        if (rigReferences == null)
        {
            player.TryGetDisplayName(out string name);
            MelonLogger.Warning($"Could not buy item with barcode {barcode} since RigReferenceCollection for player {name ?? $"with ID {player.LongId.ToString()}"} could not be found!");
            return;
        }

        RigManager rm = rigReferences.RigManager;
        Transform headTransform = rm.physicsRig.m_head;
        Transform pelvisTransform = rm.physicsRig.m_pelvis;
        var eulerAngles = pelvisTransform.eulerAngles;
        Quaternion rotation = Quaternion.Euler(new Vector3(eulerAngles.x, eulerAngles.y + -90, eulerAngles.z));
        SerializedTransform finalTransform = new SerializedTransform(headTransform.position + pelvisTransform.forward / 1.5f, rotation);

        FusionSpawning.DeferredServerSpawn(barcode, player.SmallId, finalTransform, (_, _, _, gameObject) =>
        {
            new WeaponModification(new FusionSpawning(player.SmallId)).ModifyWeapon(gameObject);

            List<Renderer> renderersHidden = DisableEnabledRenderers(gameObject);

            if (renderersHidden.Count != 0)
            {
                _RenderersHidden.Add(gameObject, renderersHidden);
            }

            PlaceInPlayerInventory(gameObject, player);
        });
    }
}
