using System;
using System.Runtime.InteropServices;
using SniffExplorer.Enums;
using SniffExplorer.Packets.Parsing;
using SniffExplorer.Packets.Types;

namespace SniffExplorer.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PowerUpdateBlock
    {
        public uint Power { get; set; }
        public byte PowerType { get; set; }
    }

    [ServerPacket(OpcodeServer.SMSG_POWER_UPDATE)]
    public struct UserClientPowerUpdate
    {
        public ObjectGuid GUID { get; set; }

        public int Count { get; set; }
        [StreamedSize("Count")]
        public PowerUpdateBlock[] Updates { get; set; }
    }
}
