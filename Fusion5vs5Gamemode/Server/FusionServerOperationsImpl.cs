using Fusion5vs5Gamemode.Utilities;
using LabFusion.SDK.Gamemodes;
using static Fusion5vs5Gamemode.Shared.Commons;

namespace Fusion5vs5Gamemode.Server;

public class FusionServerOperationsImpl : IFusionServerOperations
{
    private Gamemode FusionGamemode { get; }
    public FusionServerOperationsImpl(Gamemode fusionGamemode)
    {
        Log(fusionGamemode);
        FusionGamemode = fusionGamemode;
    }

    public bool TrySetMetadata(string key, string value)
    {
        Log(key, value);
        return FusionGamemode.TrySetMetadata(key, value);
    }

    public bool TryGetMetadata(string key, out string value)
    {
        Log(key, string.Empty);
        return FusionGamemode.TryGetMetadata(key, out value);
    }

    public bool TryRemoveMetadata(string key)
    {
        Log(key);
        return FusionGamemode.TryRemoveMetadata(key);
    }

    public bool InvokeTrigger(string value)
    {
        Log(value);
        return FusionGamemode.TryInvokeTrigger(value);
    }
}