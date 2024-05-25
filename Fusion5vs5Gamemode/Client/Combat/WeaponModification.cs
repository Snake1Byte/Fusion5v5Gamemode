using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BoneLib;
using Fusion5vs5Gamemode.Shared;
using Fusion5vs5Gamemode.Utilities;
using Il2CppSystem.Threading.Tasks;
using LabFusion.Data;
using LabFusion.Extensions;
using MelonLoader;
using SLZ.Interaction;
using SLZ.Marrow.Pool;
using UnityEngine;
using Object = UnityEngine.Object;
using static Fusion5vs5Gamemode.Shared.Commons;
using SafeActions = BoneLib.SafeActions;

namespace Fusion5vs5Gamemode.Client.Combat;

public class WeaponModification
{
    public const string ROOT_NAME = "Fusion 5vs5 Custom Objects";
    private static GameObject? _PicatinnySlot;
    private static GameObject? _MuzzleSlot;
    private static GameObject? _Dovetail;

    private static readonly Dictionary<GameObject, List<GameObject>> ModifiedWeapons = new(new GameObjectComparer());

    private readonly ISpawning _Spawning;

    private static void EmptyCollections(LevelInfo obj)
    {
        Log(obj);

        ModifiedWeapons.Clear();
    }

    static WeaponModification()
    {
        Log();

        Hooking.OnLevelInitialized += EmptyCollections;
    }

    public WeaponModification(ISpawning spawning)
    {
        Log(spawning);

        _Spawning = spawning;
    }

    public void ModifyWeapon(GameObject spawnedGo)
    {
        Log(spawnedGo);

        string? barcode = spawnedGo.GetComponent<AssetPoolee>()?.spawnableCrate._barcode;
        if (barcode == null) return;
        if (!AssetDatabase.AttachmentDatabase!.TryGetValue(barcode, out Attachments attachments)) return;
        Transform customTransform = spawnedGo.transform.Find(ROOT_NAME);
        if (customTransform != null)
        {
            customTransform.gameObject.SetActive(true);
            ResetAttachmentSlots(spawnedGo);
            return;
        }

        GameObject root = new GameObject(ROOT_NAME);
        root.transform.SetParent(spawnedGo.transform);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        foreach (string path in attachments.GameObjectsToRemove)
        {
            Transform transform = spawnedGo.transform.Find(path);
            if (transform == null) continue;
            GameObject go = transform.gameObject;
            go.SetActive(false);
        }

        InitializePicatinnySlotAsync();

        List<GameObject> addedSlots = new();
        MelonCoroutines.Start(CoWaitAndAddPicatinnySlots(root, attachments.PicatinnySlotsToAdd, () => _PicatinnySlot != null, list =>
        {
            addedSlots.AddRange(list);
            ModifiedWeapons.Add(spawnedGo, addedSlots);
        }));

        MelonCoroutines.Start(CoWaitAndAddAttachmentsToPicatinnySlots(addedSlots, attachments.PicatinnyAttachmentsToAdd, true, () => addedSlots.Count != 0));

        InitializeMuzzleSlotAsync(() =>
        {
            foreach (SerializedTransform slot in attachments.MuzzleSlotsToAdd)
            {
                AddMuzzleSlot(root, slot);
            }
        });

        SafeActions.InvokeActionSafe(attachments.CustomActionUponSpawn, spawnedGo); // TODO add this in a coroutine too
    }

    private void InitializePicatinnySlotAsync(Action? continueWith = null)
    {
        Log(continueWith!);

        if (_PicatinnySlot == null)
        {
            Utilities.SafeActions.InvokeActionSafe(_Spawning.Spawn, "Rexmeck.GunAttachments.Spawnable.45CantedRail", new SerializedTransform(Vector3.one, Quaternion.identity), (Action<GameObject>)(source =>
            {
                Transform tr = source.transform.Find("Sockets/Attachment_Rail_v2");
                if (tr == null) return;
                _PicatinnySlot = Object.Instantiate(tr.gameObject);
                _PicatinnySlot.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                _PicatinnySlot.gameObject.name = "PicatinnySlot";
                // destroy instead of despawning this, the attachment has references to the canted rail now
                Object.Destroy(source);

                SafeActions.InvokeActionSafe(continueWith);
            }));
        }
        else
        {
            SafeActions.InvokeActionSafe(continueWith);
        }
    }

    private List<GameObject> AddPicatinnySlots(GameObject root, Dictionary<string, SerializedTransform> picatinnySlotsToAdd)
    {
        Log(root, picatinnySlotsToAdd);

        List<GameObject> addedSlots = new();
        foreach (KeyValuePair<string, SerializedTransform> slot in picatinnySlotsToAdd)
        {
            GameObject? addedSlot = AddPicatinnySlot(root, slot.Value);
            if (addedSlot == null) continue;
            addedSlots.Add(addedSlot);
            addedSlot.name = slot.Key;
        }

        return addedSlots;
    }

    private IEnumerator CoWaitAndAddPicatinnySlots(GameObject root, Dictionary<string, SerializedTransform> picatinnySlotsToAdd, Func<bool> startUponCondition, Action<List<GameObject>> handleReturnValue)
    {
        while (!startUponCondition.Invoke())
            yield return null;
        List<GameObject> returnValue = AddPicatinnySlots(root, picatinnySlotsToAdd);
        SafeActions.InvokeActionSafe(handleReturnValue, returnValue);
    }

    private GameObject? AddPicatinnySlot(GameObject root, SerializedTransform transform)
    {
        Log(root, transform);

        if (_PicatinnySlot == null) return null;
        GameObject? obj = AddGameObject(_PicatinnySlot, "", root);
        if (obj == null) return null;
        obj.transform.localPosition = transform.position;
        obj.transform.localRotation = transform.rotation;
        return obj;
    }

    private void AddAttachmentsToPicatinnySlots(List<GameObject> slots, Dictionary<string, string> attachmentsToAdd, bool overwriteExisting)
    {
        Log(slots, attachmentsToAdd, overwriteExisting);

        foreach (var pair in attachmentsToAdd)
        {
            string attachmentBarcodeToSpawn = pair.Key;
            string slotNameToAddTo = pair.Value;
            try
            {
                GameObject slot = slots.First(go => go.name.Equals(slotNameToAddTo));
                AddAttachmentToPicatinnySlot(slot, attachmentBarcodeToSpawn, overwriteExisting);
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    private IEnumerator CoWaitAndAddAttachmentsToPicatinnySlots(List<GameObject> slots, Dictionary<string, string> attachmentsToAdd, bool overwriteExisting, Func<bool> startUponCondition)
    {
        while (!startUponCondition.Invoke())
            yield return null;
        AddAttachmentsToPicatinnySlots(slots, attachmentsToAdd, overwriteExisting);
    }

    private void AddAttachmentToPicatinnySlot(GameObject slot, string attachmentBarcode, bool overwriteExisting)
    {
        Log(slot, attachmentBarcode, overwriteExisting);

        GameObject? alreadyExisting = GetAttachment(slot);
        if (alreadyExisting != null)
        {
            if (overwriteExisting)
            {
                RemoveAttachmentFromPicatinnySlot(slot, true);
                MelonCoroutines.Start(CoWaitAndAddAttachmentToPicatinnySlot(slot, attachmentBarcode, false, () => GetAttachment(slot) == null));
            }
        }
        else
        {
            Utilities.SafeActions.InvokeActionSafe(_Spawning.Spawn, attachmentBarcode, new SerializedTransform(slot.transform.position, slot.transform.rotation), (Action<GameObject>)(spawnedAttachmentGo =>
            {
                try
                {
                    List<Renderer> disabledRenderers = DisableEnabledRenderers(spawnedAttachmentGo);
                    KeyReciever receiver = slot.GetComponent<KeyReciever>();
                    InteractableHost host = spawnedAttachmentGo.GetComponent<InteractableHost>();
                    if (receiver == null || host == null) return;
                    receiver.OnInteractableHostEnter(host);
                    MelonCoroutines.Start(Commons.CoRunUponCondition(() =>
                    {
                        foreach (Renderer disabledRenderer in disabledRenderers)
                        {
                            disabledRenderer.enabled = true;
                        }
                    }, () => IsAttachmentSlotted(slot)));
                }
                catch (InvalidOperationException)
                {
                }
            }));
        }
    }

    private IEnumerator CoWaitAndAddAttachmentToPicatinnySlot(GameObject slot, string attachmentBarcode, bool overwriteExisting, Func<bool> startUponCondition)
    {
        while (!startUponCondition.Invoke())
            yield return null;
        AddAttachmentToPicatinnySlot(slot, attachmentBarcode, overwriteExisting);
    }

    private void RemoveAttachmentsFromPicatinnySlots(List<GameObject> slots, bool despawn)
    {
        Log(slots, despawn);

        foreach (var slot in slots)
        {
            RemoveAttachmentFromPicatinnySlot(slot, despawn);
        }
    }

    private void RemoveAttachmentFromPicatinnySlot(GameObject slot, bool despawn)
    {
        Log(slot, despawn);

        GameObject? attachment = GetAttachment(slot);
        if (attachment == null) return;
        SimpleGripEvents? simpleGripEvents = attachment.GetComponentInChildren<SimpleGripEvents>();
        if (simpleGripEvents == null) return;
        simpleGripEvents.OnMenuTapDown?.Invoke();
        if (despawn)
        {
            AssetPoolee poolee = attachment.GetComponent<AssetPoolee>();
            if (poolee == null) return;
            _Spawning.Despawn(poolee);
        }
    }

    private void ResetAttachmentSlots(GameObject weapon)
    {
        Log(weapon);

        List<GameObject> slots = ModifiedWeapons[weapon];
        foreach (GameObject slot in slots)
        {
            if (SlotNeedsReset(weapon, slot)) RemoveAttachmentFromPicatinnySlot(slot, true);
        }

        AssetPoolee assetPoolee = weapon.GetComponent<AssetPoolee>();
        if (!AssetDatabase.AttachmentDatabase!.TryGetValue(assetPoolee.spawnableCrate._barcode, out Attachments attachments))
            return;
        MelonCoroutines.Start(CoWaitAndAddAttachmentsToPicatinnySlots(slots, attachments.PicatinnyAttachmentsToAdd, false, () =>
        {
            foreach (GameObject slot in slots)
            {
                if (SlotNeedsReset(weapon, slot)) return false;
            }

            return true;
        }));
    }

    private bool SlotNeedsReset(GameObject weapon, GameObject slot)
    {
        Log(weapon, slot);

        GameObject? attachment = GetAttachment(slot);
        if (attachment == null) return false;
        AssetPoolee assetPoolee = weapon.GetComponent<AssetPoolee>();
        if (!AssetDatabase.AttachmentDatabase!.TryGetValue(assetPoolee.spawnableCrate._barcode, out Attachments attachments))
            return false;
        foreach (KeyValuePair<string, string> pair in attachments.PicatinnyAttachmentsToAdd)
        {
            string attachmentBarcode = pair.Key;
            string slotName = pair.Value;

            if (slotName.Equals(slot.name) && attachment.GetComponent<AssetPoolee>().spawnableCrate._barcode.Equals(attachmentBarcode))
            {
                return false;
            }

            return true;
        }

        return false;
    }

    private void InitializeMuzzleSlotAsync(Action? continueWith = null)
    {
        // TODO
    }

    private void AddMuzzleSlot(GameObject root, SerializedTransform transform)
    {
        // TODO
    }

    private GameObject? AddGameObject(GameObject source, string path, GameObject target)
    {
        Log(source, path, target);
        Transform transform;
        if (string.IsNullOrEmpty(path))
        {
            transform = source.transform;
        }
        else
        {
            transform = source.transform.Find(path);
        }

        if (transform == null) return null;
        return Object.Instantiate(transform.gameObject, target.transform, false);
    }

    private GameObject? GetAttachment(GameObject slot)
    {
        Log(slot);

        if (slot == null) return null;
        GameObject? attachment = slot.transform.Find("ATTACHMENTV2")?.gameObject;
        return attachment;
    }

    private bool IsAttachmentSlotted(GameObject slot)
    {
        Log(slot);

        if (slot == null) return false;
        KeyReciever? receiver = slot.GetComponent<KeyReciever>();
        if (receiver == null) return false;
        return !receiver.enabled;
    }
}
