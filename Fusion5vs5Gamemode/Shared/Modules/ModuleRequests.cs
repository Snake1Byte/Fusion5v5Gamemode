using Fusion5vs5Gamemode.Shared;
using Fusion5vs5Gamemode.Shared.Modules;
using LabFusion.Network;
using static Fusion5vs5Gamemode.Shared.Commons;

namespace Fusion5vs5Gamemode.Client;

public static class ModuleRequests
{
    public static void GenericRequestToServer(string genericRequest)
    {
        Log(genericRequest);

        if (NetworkInfo.HasServer)
        {
            using (var writer = FusionWriter.Create())
            {
                using (var data = GenericClientRequestData.Create(genericRequest))
                {
                    writer.Write(data);
                    using (var message = FusionMessage.ModuleCreate<GenericClientRequestHandler>(writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }
    }

    public static void BroadcastDeferredItemSpawned(ushort syncId, byte owner, string barcode)
    {
        Log(syncId, owner, barcode);

        if (!NetworkInfo.IsServer) return;

        using (var writer = FusionWriter.Create(DeferredServerSpawnData.Size))
        {
            using (var data = DeferredServerSpawnData.Create(syncId, owner, barcode))
            {
                writer.Write(data);
                using (var message = FusionMessage.ModuleCreate<DeferredServerSpawnHandler>(writer))
                {
                    MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                }
            }
        }
    }
}
