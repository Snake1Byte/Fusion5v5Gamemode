using LabFusion.Network;
using LabFusion.SDK.Gamemodes;

namespace Fusion5vs5Gamemode.Shared.Modules;

internal class GenericClientRequestHandler : ModuleMessageHandler
{
    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        if (NetworkInfo.IsServer && Client.Client.Instance != null)
        {
            using (var reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<GenericClientRequestData>())
                {
                    ModuleMessages.InvokeGenericClientRequest(data.Value);
                }
            }
        }
    }
}
