using System;
using SniffExplorer.Enums;
using SniffExplorer.Packets.Parsing;
using SniffExplorer.Packets.Types;

namespace SniffExplorer.Packets
{
    public struct CriteriaProgress
    {
        public uint ID { get; set; }
        public ulong Quantity { get; set; }
        public ObjectGuid Player { get; set; }
        [PackedField]
        public DateTime Date { get; set; }
        public DateTime TimeFromStart { get; set; }
        public DateTime TimeFromCreate { get; set; }
        [BitField(4)]
        public byte Flags { get; set; }
    }

    public struct EarnedAchievement
    {
        public uint ID { get; set; }
        [PackedField]
        public DateTime Date { get; set; }
        public ObjectGuid Owner { get; set; }
        public int VirtualRealmAddress { get; set; }
        public int NativeRealmAddress { get; set; }
    }

    [Packet(OpcodeServer.SMSG_CRITERIA_UPDATE)]
    public struct ClientCriteriaUpdate
    {
        public uint ID { get; set; }
        public ulong Quantity { get; set; }
        public ObjectGuid Player { get; set; }
        public int Flags { get; set; }
        [PackedField]
        public DateTime Date { get; set; }
        public DateTime TimeFromStart { get; set; }
        public DateTime TimeFromCreate { get; set; }
    }

    [Packet(OpcodeServer.SMSG_ACCOUNT_CRITERIA_UPDATE)]
    public struct ClientAccountCriteriaUpdate
    {
        public CriteriaProgress Progress { get; set; }
    }

    [Packet(OpcodeServer.SMSG_ALL_ACCOUNT_CRITERIA)]
    public struct ClientAllAccountCriteria
    {
        [StreamedSize]
        public CriteriaProgress[] Progress { get; set; }
    }

    [Packet(OpcodeServer.SMSG_ACHIEVEMENT_EARNED)]
    public struct ClientAchievementEarned
    {
        public ObjectGuid Sender { get; set; }
        public ObjectGuid Earner { get; set; }
        public int AchievementID { get; set; }
        [PackedField]
        public DateTime Time { get; set; }
        public uint EarnerNativeRealm { get; set; }
        public uint EarnerVirtualRealm { get; set; }
        [BitField]
        public bool Initial { get; set; }
    }

    public struct AllAchievements
    {
        public int EarnedCount { get; set; }
        public int ProgressCount { get; set; }

        [StreamedSize("EarnedCount")]
        public EarnedAchievement[] Earned { get; set; }
        [StreamedSize("ProgressCount")]
        public CriteriaProgress[] Progress { get; set; }
    }

    [Packet(OpcodeServer.SMSG_RESPOND_INSPECT_ACHIEVEMENTS)]
    public struct ClientRespondInspectAchievements
    {
        public ObjectGuid Player { get; set; }
        public AllAchievements Data { get; set; }
    }
}
