using System.ComponentModel;
using SniffExplorer.Core;
using SniffExplorer.Core.Packets.Parsing.Attributes;
using V22996 = SniffExplorer.Legion.Enums.V22996;

namespace SniffExplorer.Legion.Packets
{
    [Packet(typeof(V22996.OpcodeClient), "CMSG_AUTH_SESSION"), TargetBuild(22996)]
    public struct UserClientAuthSession
    {
        public ulong DosResponse { get; set; }
        public ushort Build { get; set; }
        public byte BuildType { get; set; }
        public uint RegionID { get; set; }
        public uint BattlegroupID { get; set; }
        public uint RealmID { get; set; }
        [Size(16)]
        public byte[] LocalChallenge { get; set; }
        [Size(24)]
        public byte[] Digest { get; set; }
        public bool UsesIPv6 { get; set; }

        [Size]
        public byte[] RealmJoinTicket { get; set; }
    }
}
