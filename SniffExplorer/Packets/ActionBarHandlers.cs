﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SniffExplorer.Enums;
using SniffExplorer.Packets.Parsing;

namespace SniffExplorer.Packets
{
    public struct ActionButton
    {
        public uint Action { get; set; }
        public uint Type { get; set; }
    }

    [ServerPacket(OpcodeServer.SMSG_UPDATE_ACTION_BUTTONS)]
    public struct ClientUpdateActionButtons
    {
        [FixedSize(132)]
        public ActionButton[] Buttons { get; set; }
        public byte Reason { get; set; }
    }

    [ClientPacket(OpcodeClient.CMSG_SET_ACTION_BUTTON)]
    public struct UserClientSetActionButton
    {
        private ActionButton Button { get; set; }
        public byte Index { get; set; }
    }
}