using SniffExplorer.Core;
using SniffExplorer.Core.Packets;
using SniffExplorer.Core.Attributes;
using SniffExplorer.Core.Packets.Types;
using System.ComponentModel;
using System.Text;

namespace SniffExplorer.Cataclysm.Packets
{
    [Packet(Opcodes.SMSG_SPELL_PERIODIC_AURA_LOG, PacketDirection.ServerToClient), TargetBuild(15595)]
    public struct UserClientSpellPeriodicLogAura
    {
        public ObjectGuid64 Target { get; set; }
        public ObjectGuid64 Caster { get; set; }

        [Meta(MetaDataType.Spell)]
        public uint SpellID { get; set; }

        public uint Count { get; set; }
        public uint AuraType { get; set; }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class PeriodicDamageInfo
        {
            public uint Damage { get; set; }
            public int Overkill { get; set; }
            public uint SchoolMask { get; set; }
            public int Absorb { get; set; }
            public int Resist { get; set; }
            public byte Critical { get; set; }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append($"Damage: {Damage}");
                if (Overkill > 0)
                    sb.Append($" (Excess {Overkill})");
                if (Absorb > 0)
                    sb.Append($" (Absorbed {Absorb})");
                if (Resist > 0)
                    sb.Append($" (Resisted {Resist})");
                if (Critical != 0)
                    sb.Append(" (Critical)");
                return sb.ToString();
            }
        }

        [Conditional("AuraType", ConditionType.Equal, 3u, 89u)]
        public PeriodicDamageInfo DamageInfo { get; set; }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class PeriodicHealInfo
        {
            public uint HealAmount { get; set; }
            public int Overheal { get; set; }
            public int Absorb { get; set; }
            public byte Critical { get; set; }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append($"Heal: {HealAmount}");
                if (Overheal > 0)
                    sb.Append($" (Excess {Overheal})");
                if (Absorb > 0)
                    sb.Append($" (Absorbed {Absorb})");
                if (Critical != 0)
                    sb.Append(" (Critical)");
                return sb.ToString();
            }
        }

        [Conditional("AuraType", ConditionType.Equal, 8u, 20u)]
        public PeriodicHealInfo HealInfo { get; set; }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class PeriodicEnergizeInfo
        {
            public uint PowerType { get; set; }
            public uint Amount { get; set; }

            public override string ToString()
            {
                return $"Energize by {Amount} (Type {PowerType})";
            }
        }

        [Conditional("AuraType", ConditionType.Equal, 21u, 24u)]
        public PeriodicEnergizeInfo Energize { get; set; }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class PeriodicManaLeechInfo
        {
            public uint PowerType { get; set; }
            public uint Amount { get; set; }
            public float Multiplier { get; set; }

            public override string ToString() => $"Energize by {Amount}x{Multiplier} (Type {PowerType})";
        }

        [Conditional("AuraType", ConditionType.Equal, 64u)]
        public PeriodicManaLeechInfo ManaLeech { get; set; }
    }

    [Packet(Opcodes.SMSG_SPELL_GO, PacketDirection.ServerToClient), TargetBuild(15595)]
    public struct SpellGo
    {
        public ObjectGuid64 Caster { get; set; }
        public ObjectGuid64 UnitCaster { get; set; }

        public byte CastCount { get; set; }
        [Meta(MetaDataType.Spell)]
        public uint SpellID { get; set; }
        public uint Flags { get; set; }
        public uint Timer { get; set; }
        public uint Timestamp { get; set; }

        [Size(Method = SizeMethod.InPlace, Param = 8), RawGuid]
        public ObjectGuid64[] Targets { get; set; }

        public class TargetMissInfo
        {
            [RawGuid]
            public ObjectGuid64 GUID { get; set; }
            public byte Reason { get; set; }

            [Conditional("Reason", ConditionType.Equal, (byte)11)]
            public byte ReflectReason { get; set; }
        }

        [Size(Method = SizeMethod.InPlace, Param = 8)]
        public TargetMissInfo[] MissTargets { get; set; }
    }
}
