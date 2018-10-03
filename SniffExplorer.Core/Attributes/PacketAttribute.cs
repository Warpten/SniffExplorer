using SniffExplorer.Core.Packets;
using System;

namespace SniffExplorer.Core.Attributes
{
    /// <summary>
    /// Use this attribute to decorate structures that act as opcode parsers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class PacketAttribute : Attribute
    {
        public PacketDirection Direction { get; set; }
        public Opcodes Opcode { get; }

        public PacketAttribute(Opcodes opcode, PacketDirection direction)
        {
            Opcode = opcode;
            Direction = direction;
        }
    }
}
