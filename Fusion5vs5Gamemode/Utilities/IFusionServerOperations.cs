namespace Fusion5vs5Gamemode.Utilities;

public interface IFusionServerOperations
{
    bool TrySetMetadata(string key, string value);
    bool TryGetMetadata(string key, out string value);
    bool TryRemoveMetadata(string key);
    bool InvokeTrigger(string value);
}