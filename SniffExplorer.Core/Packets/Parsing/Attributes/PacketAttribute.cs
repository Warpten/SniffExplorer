using System;

namespace SniffExplorer.Core.Packets.Parsing.Attributes
{
    /// <summary>
    /// Use this attribute to decorate structures that act as opcode parsers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class PacketAttribute : Attribute
    {
        public object Opcode { get; }

        public PacketAttribute(Type enumType, string stringRepresentation)
        {
            Opcode = Enum.Parse(enumType, stringRepresentation, false);
        }
    }
}
