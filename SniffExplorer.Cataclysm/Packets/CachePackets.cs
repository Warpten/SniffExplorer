using SniffExplorer.Core;
using SniffExplorer.Core.Packets;
using SniffExplorer.Core.Attributes;
using SniffExplorer.Core.Packets.Types;

namespace SniffExplorer.Cataclysm.Packets
{
    [Packet(Opcodes.CMSG_QUERY_PLAYER_NAME, PacketDirection.ClientToServer), TargetBuild(15595)]
    public struct UserClientQueryPlayerName
    {
        [RawGuid]
        public ObjectGuid64 Guid { get; set; }
    }

    [Packet(Opcodes.SMSG_QUERY_PLAYER_NAME_RESPONSE, PacketDirection.ServerToClient), TargetBuild(15595)]
    public struct ClientQueryPlayerNameResponse
    {
        public ObjectGuid64 Guid { get; set; }
        public byte HasData { get; set; }

        [StopIf("HasData", ConditionType.Different, (byte)0)]

        public string Name { get; set; }
        public string RealmName { get; set; }
        public byte Race { get; set; }
        public byte Gender { get; set; }
        public byte Class { get; set; }
        public byte IsNameDeclined { get; set; }
        [Conditional("IsNameDeclined", ConditionType.Equal, (byte)1), Size(Method = SizeMethod.FixedSize, Param = 5)]
        public string[] DeclinedName { get; set; }
    }

    [Packet(Opcodes.SMSG_QUERY_CREATURE_RESPONSE, PacketDirection.ServerToClient), TargetBuild(15595)]
    public struct ClientQueryCreatureResponse
    {
        public uint Entry { get; set; }

        [StopIf("Entry", ConditionType.And, 0x80000000)]

        [Size(Method = SizeMethod.FixedSize, Param = 8)]
        public string[] Name { get; set; }
        public string SubName { get; set; }
        public string IconName { get; set; }
        [Size(Method = SizeMethod.FixedSize, Param = 2)]
        public uint[] TypeFlags { get; set; }
        public uint Type { get; set; }
        public uint Family { get; set; }
        public uint Rank { get; set; }
        [Size(Method = SizeMethod.FixedSize, Param = 2)]
        public uint[] KillCredits { get; set; }
        [Size(Method = SizeMethod.FixedSize, Param = 4)]
        public uint[] ModelIDs { get; set; }
        public float HealthModifier { get; set; }
        public float PowerModifier { get; set; }
        public byte RacialLeader { get; set; }
        [Size(Method = SizeMethod.FixedSize, Param = 6)]
        public uint[] QuestItems { get; set; }
        public uint Expansion { get; set; }
        public uint ExpansionUnknown { get; set; }

    }
}
