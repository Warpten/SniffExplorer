using System.ComponentModel;
using SniffExplorer.Core.Packets.Parsing;

namespace SniffExplorer.Core.Packets.Types
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ObjectGuid
    {
        public ulong LowPart { get; protected set; }
        public ulong HighPart { get; protected set; }

        public virtual byte SubType => (byte) (HighPart & 0x3F);
        public virtual ushort RealmId => (ushort) ((HighPart >> 42) & 0x1FFF);
        public virtual uint ServerId => (uint) ((LowPart >> 40) & 0xFFFFFF);
        public virtual ushort MapId => (ushort) ((HighPart >> 29) & 0x1FFF);
        public virtual uint Entry => (uint) ((HighPart >> 6) & 0x7FFFFF);
        public virtual ulong Low => LowPart & 0xFFFFFFFFFF;
        public virtual HighGuidType HighType => (HighGuidType)((HighPart >> 58) & 0x3F);

        public virtual void Read(PacketReader reader)
        {
            var loMask = reader.ReadByte();
            var hiMask = reader.ReadByte();

            LowPart = reader.ReadPackedUInt64(loMask);
            HighPart = reader.ReadPackedUInt64(hiMask);
        }

        public bool HasEntry()
        {
            switch (HighType)
            {
                case HighGuidType.Creature:
                case HighGuidType.GameObject:
                case HighGuidType.Pet:
                case HighGuidType.Vehicle:
                case HighGuidType.AreaTrigger:
                    return true;
                default:
                    return false;
            }
        }

        public override string ToString()
        {
            if (Low == 0 && HighPart == 0)
                return "Full: 0x0";

            if (HasEntry())
            {
                return
                    $"Full: 0x{HighPart.ToString("X16")}{LowPart.ToString("X16")} {HighType}/{SubType} R{RealmId}/S{ServerId} Map: {MapId} Entry: {Entry} Low: {Low}";
            }

            return
                $"Full: 0x{HighPart.ToString("X16")}{Low.ToString("X16")} {HighType}/{SubType} R{RealmId}/S{ServerId} Map: {MapId} Low: {Low}";
        }
    }
}
