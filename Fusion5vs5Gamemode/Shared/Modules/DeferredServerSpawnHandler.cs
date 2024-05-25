using LabFusion.Network;

namespace Fusion5vs5Gamemode.Shared.Modules;

internal class DeferredServerSpawnHandler : ModuleMessageHandler
{
    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        if (NetworkInfo.IsServer && Client.Client.Instance != null)
        {
            using (var reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<DeferredServerSpawnData>())
                {
                    ModuleMessages.InvokeServerSpawned(data.SyncId, data.Owner, data.Barcode);
                }
            }
        }
    }
}
