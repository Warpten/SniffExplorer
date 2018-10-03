using SniffExplorer.Core;
using SniffExplorer.Core.Packets;
using SniffExplorer.Core.Attributes;

namespace SniffExplorer.Cataclysm.Packets
{
    [Packet(Opcodes.SMSG_GUILD_PARTY_STATE, PacketDirection.ServerToClient), TargetBuild(15595)]
    public struct GuildPartyState
    {
        [BitField]
        public bool IsGuildGroup { get; set; }
        public float Multiplier { get; set; }
        public uint GuildMemberCount { get; set; }
        public uint NeededGuildMemberCount { get; set; }
    }

    // [Packet(typeof(V15595.OpcodeClient), "CMSG_GUILD_OFFICER_REMOVE_MEMBER"), TargetBuild(15595)]
    // public struct GuildOfficerRemoveMember
    // {
    //     [StreamedGuid(BitStream = new byte[] { 6, 5, 4, 0, 1, 3, 7, 2 }, ByteStream = new byte[] { 2, 6, 5, 7, 1, 4, 3, 0 })]
    //     public ObjectGuid Target { get; set; }
    // }

    [Packet(Opcodes.CMSG_GUILD_UPDATE_MOTD_TEXT, PacketDirection.ClientToServer), TargetBuild(15595)]
    public struct GuildMOTD
    {
        [Size(Method = SizeMethod.InPlace, Param = 11)]
        public string Body { get; set; }
    }

    [Packet(Opcodes.CMSG_GUILD_CHANGE_NAME_REQUEST, PacketDirection.ClientToServer), TargetBuild(15595)]
    public struct GuildNameChange
    {
        [Size(Method = SizeMethod.InPlace, Param = 8)]
        public string Body { get; set; }
    }

    // [Packet(typeof(V15595.OpcodeClient), "CMSG_REQUEST_GUILD_XP"), TargetBuild(15595)]
    // public struct GuildRequestGuildXP
    // {
    //     [StreamedGuid(BitStream = new byte[] { 2, 1, 0, 5, 4, 7, 6, 3 }, ByteStream = new byte[] { 7, 2, 3, 6, 1, 5, 0, 4 })]
    //     public ObjectGuid Target { get; set; }
    // }
}
