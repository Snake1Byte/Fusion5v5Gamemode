using System;
using LabFusion.Data;
using LabFusion.Network;

namespace Fusion5vs5Gamemode.Shared.Modules;

internal class DeferredServerSpawnData : IFusionSerializable, IDisposable
{
    public const int Size = sizeof(byte) + sizeof(ushort);
    public ushort SyncId;
    public byte Owner;
    public string Barcode;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(SyncId);
        writer.Write(Owner);
        writer.Write(Barcode);
    }

    public void Deserialize(FusionReader reader)
    {
        SyncId = reader.ReadUInt16();
        Owner = reader.ReadByte();
        Barcode = reader.ReadString();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public static DeferredServerSpawnData Create(ushort syncId, byte owner, string barcode)
    {
        return new DeferredServerSpawnData { SyncId = syncId, Owner = owner, Barcode = barcode};
    }
}
