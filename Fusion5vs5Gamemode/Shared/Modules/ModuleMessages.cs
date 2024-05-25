using System;

namespace Fusion5vs5Gamemode.Shared.Modules;

public static class ModuleMessages
{
    public static Action<string>? GenericClientRequest;
    public static Action<ushort, byte, string>? DeferredItemSpawned;

    internal static void InvokeGenericClientRequest(string eventTrigger)
    {
        BoneLib.SafeActions.InvokeActionSafe(GenericClientRequest, eventTrigger);
    }

    internal static void InvokeServerSpawned(ushort syncId, byte owner, string barcode)
    {
        Utilities.SafeActions.InvokeActionSafe(DeferredItemSpawned, syncId, owner, barcode);
    } 
}
