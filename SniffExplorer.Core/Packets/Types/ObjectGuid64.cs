using System.ComponentModel;
using System.Text;
using SniffExplorer.Core.Packets.Parsing;

namespace SniffExplorer.Core.Packets.Types
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public sealed class ObjectGuid64 : IObjectGuid
    {
        public ulong Value { get; private set; }

        public bool IsEmpty() => Value != 0L;

        public uint Entry => HasEntry() ? (uint)((Value >> 32) & 0x00000000000FFFFFL) : 0;
        public ulong Low => (uint)(Value & 0x00000000FFFFFFFFL);
        public HighGuidType HighType
        {
            get
            {
                //! TOOD Fix this mess
                var oldType = ((Value >> 48) & 0x0000FFFF);
                oldType = ((oldType == 0xF101 || oldType == 0xF102) ? oldType : (ulong)(((int)oldType >> 4) & 0x00000FFF));
                switch (oldType)
                {
                    case 0x0400: return HighGuidType.Item;
                    case 0x0038:
                    case 0x0050:
                    case 0x0000: return HighGuidType.Player;
                    case 0x0F11: return HighGuidType.GameObject;
                    case 0x0F12: return HighGuidType.Transport;
                    case 0x0F13: return HighGuidType.Creature;
                    case 0x0F14: return HighGuidType.Pet;
                    case 0x0F15: return HighGuidType.Vehicle;
                    case 0x0F10: return HighGuidType.DynamicObject;
                    case 0xF101: return HighGuidType.Corpse;
                    case 0xF102: return HighGuidType.AreaTrigger;
                    case 0x01F1: return HighGuidType.CallForHelp;
                    // case 0x01FC: return HighGuidType.MoTransport;
                    // case 0x01F4: return HighGuidType.Instance;
                    case 0x01F5: return HighGuidType.RaidGroup;
                    case 0x01FF: return HighGuidType.Guild;
                }

                return (HighGuidType)oldType;
            }
        }

        public void Read(PacketReader reader)
        {
            Value = reader.ReadUInt64();
        }

        public void ReadPacked(PacketReader reader)
        {
            var loMask = reader.ReadByte();

            Value = reader.ReadPackedUInt64(loMask);
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
            if (Value == 0L)
                return "Full: 0x0";

            var str = new StringBuilder();
            str.Append($"GUID Full: 0x{Value:X16} Type: {HighType}");
            if (HasEntry())
                str.Append(HighType == HighGuidType.Pet ? $" Pet number: {Entry}" : $" Entry: {Entry}");
            str.Append($" Low: {Low}");
            return str.ToString();
        }
    }
}
