using System;
using SniffExplorer.Enums;
using SniffExplorer.Packets.Parsing;
using SniffExplorer.Packets.Types;

namespace SniffExplorer.Packets
{
    [Packet(OpcodeServer.SMSG_ACCOUNT_DATA_TIMES)]
    public struct ClientAccountDataTimes
    {
        public ObjectGuid GUID { get; set; }
        public DateTime ServerTime { get; set; }
        [FixedSize(8)]
        public DateTime[] AccountTimes { get; set; }
    }

    [Packet(OpcodeClient.CMSG_REQUEST_ACCOUNT_DATA)]
    public struct UserClientRequestAccountData
    {
        public ObjectGuid GUID { get; set; }
        [BitField(3)]
        public ushort DataType { get; set; }
    }
}
