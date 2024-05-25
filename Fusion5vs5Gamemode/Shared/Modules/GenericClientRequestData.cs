using System;
using LabFusion.Data;
using LabFusion.Network;

namespace Fusion5vs5Gamemode.Shared.Modules;

internal class GenericClientRequestData : IFusionSerializable, IDisposable
{
    public string Value = null!;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(Value);
    }

    public void Deserialize(FusionReader reader)
    {
        Value = reader.ReadString();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public static GenericClientRequestData Create(string value)
    {
        return new GenericClientRequestData { Value = value };
    }
}