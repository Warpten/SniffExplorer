using System;
using System.ComponentModel;
using SniffExplorer.Core;
using SniffExplorer.Core.Packets.Parsing.Attributes;
using SniffExplorer.Core.Packets.Types;
using V22996 = SniffExplorer.Legion.Enums.V22996;

namespace SniffExplorer.Legion.Packets
{
    [Packet(typeof(V22996.OpcodeServer), "SMSG_ACCOUNT_DATA_TIMES"), TargetBuild(22996)]
    public struct ClientAccountDataTimes
    {
        public ObjectGuid128 GUID { get; set; }
        [TypeConverter(typeof(DateTimeConverter))]
        public DateTime ServerTime { get; set; }
        [Size(Method = SizeMethod.FixedSize, Param = 8), TypeConverter(typeof(DateTimeConverter))]
        public DateTime[] AccountTimes { get; set; }
    }

    [Packet(typeof(V22996.OpcodeClient), "CMSG_REQUEST_ACCOUNT_DATA"), TargetBuild(22996)]
    public struct UserClientRequestAccountData
    {
        public ObjectGuid128 GUID { get; set; }
        [BitField(3)]
        public ushort DataType { get; set; }
    }
}
