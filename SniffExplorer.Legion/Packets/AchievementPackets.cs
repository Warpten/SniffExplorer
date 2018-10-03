using System;
using System.ComponentModel;
using SniffExplorer.Core;
using SniffExplorer.Core.Packets.Parsing.Attributes;
using SniffExplorer.Core.Packets.Types;
using V22996 = SniffExplorer.Legion.Enums.V22996;

namespace SniffExplorer.Legion.Packets
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct CriteriaProgress
    {
        public uint ID { get; set; }
        public ulong Quantity { get; set; }
        public ObjectGuid128 GUID { get; set; }
        [PackedField, TypeConverter(typeof(DateTimeConverter))]
        public DateTime Date { get; set; }
        [TypeConverter(typeof(DateTimeConverter))]
        public DateTime TimeFromStart { get; set; }
        [TypeConverter(typeof(DateTimeConverter))]
        public DateTime TimeFromCreate { get; set; }
        [BitField(4)]
        public byte Flags { get; set; }

        public override string ToString() => $"Criteria #{ID}";
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct AllAchievements
    {
        [Browsable(false)]
        public int EarnedCount { get; set; }
        [Browsable(false)]
        public int ProgressCount { get; set; }

        [Size(Method = SizeMethod.StreamedProperty, Param = "EarnedCount")]
        public EarnedAchievement[] Earned { get; set; }
        [Size(Method = SizeMethod.StreamedProperty, Param = "ProgressCount")]
        public CriteriaProgress[] Progress { get; set; }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct EarnedAchievement
    {
        public uint ID { get; set; }
        [PackedField, TypeConverter(typeof(DateTimeConverter))]
        public DateTime Date { get; set; }
        public ObjectGuid128 Owner { get; set; }
        public int VirtualRealmAddress { get; set; }
        public int NativeRealmAddress { get; set; }
    }

    [Packet(typeof(V22996.OpcodeServer), "SMSG_CRITERIA_UPDATE"), TargetBuild(22996)]
    public struct ClientCriteriaUpdate
    {
        public uint ID { get; set; }
        public ulong Quantity { get; set; }
        public ObjectGuid128 Player { get; set; }
        public int Flags { get; set; }
        [PackedField, TypeConverter(typeof(DateTimeConverter))]
        public DateTime Date { get; set; }
        [TypeConverter(typeof(DateTimeConverter))]
        public DateTime TimeFromStart { get; set; }
        [TypeConverter(typeof(DateTimeConverter))]
        public DateTime TimeFromCreate { get; set; }
    }

    [Packet(typeof(V22996.OpcodeServer), "SMSG_ACCOUNT_CRITERIA_UPDATE"), TargetBuild(22996)]
    public struct ClientAccountCriteriaUpdate
    {
        public CriteriaProgress Progress { get; set; }
    }

    [Packet(typeof(V22996.OpcodeServer), "SMSG_ALL_ACCOUNT_CRITERIA"), TargetBuild(22996)]
    public struct ClientAllAccountCriteria
    {
        [Size, TypeConverter(typeof(ExpandableObjectConverter))]
        public CriteriaProgress[] Progress { get; set; }
    }

    [Packet(typeof(V22996.OpcodeServer), "SMSG_ACHIEVEMENT_EARNED"), TargetBuild(22996)]
    public struct ClientAchievementEarned
    {
        [TypeConverter(typeof(DateTimeConverter))]
        public ObjectGuid128 Sender { get; set; }
        [TypeConverter(typeof(DateTimeConverter))]
        public ObjectGuid128 Earner { get; set; }
        public int AchievementID { get; set; }
        [PackedField, TypeConverter(typeof(DateTimeConverter))]
        public DateTime Time { get; set; }
        public uint EarnerNativeRealm { get; set; }
        public uint EarnerVirtualRealm { get; set; }
        public bool Initial { get; set; }
    }

    [Packet(typeof(V22996.OpcodeServer), "SMSG_RESPOND_INSPECT_ACHIEVEMENTS"), TargetBuild(22996)]
    public struct ClientRespondInspectAchievements
    {
        public ObjectGuid128 Player { get; set; }
        public AllAchievements Data { get; set; }
    }
}
