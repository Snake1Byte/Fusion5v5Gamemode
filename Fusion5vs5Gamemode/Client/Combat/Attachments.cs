using System;
using System.Collections.Generic;
using LabFusion.Data;
using UnityEngine;

namespace Fusion5vs5Gamemode.Client.Combat;

public class Attachments
{
    public readonly Dictionary<string, SerializedTransform> PicatinnySlotsToAdd = new();
    public readonly List<SerializedTransform> MuzzleSlotsToAdd = new();
    public readonly Dictionary<string, string> PicatinnyAttachmentsToAdd = new();
    public readonly List<string> GameObjectsToRemove = new();
    public Action<GameObject> CustomActionUponSpawn;
}