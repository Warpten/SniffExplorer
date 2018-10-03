using System.ComponentModel;
using SniffExplorer.Core;
using SniffExplorer.Core.Packets.Parsing.Attributes;
using V22996 = SniffExplorer.Legion.Enums.V22996;

namespace SniffExplorer.Legion.Packets
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct ActionButton
    {
        public uint Action { get; set; }
        public uint Type { get; set; }

        public override string ToString() => $"Type: {Type} Action: {Action}";
    }

    [Packet(typeof(V22996.OpcodeServer), "SMSG_UPDATE_ACTION_BUTTONS"), TargetBuild(22996)]
    public struct ClientUpdateActionButtons
    {
        [Size(Method = SizeMethod.FixedSize, Param = 132)]
        public ActionButton[] Buttons { get; set; }
        public byte Reason { get; set; }
    }

    [Packet(typeof(V22996.OpcodeClient), "CMSG_SET_ACTION_BUTTON"), TargetBuild(22996)]
    public struct UserClientSetActionButton
    {
        public ActionButton Button { get; set; }
        public byte Index { get; set; }
    }
}
