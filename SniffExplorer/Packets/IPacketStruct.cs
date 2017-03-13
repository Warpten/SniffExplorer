using System;

namespace SniffExplorer.Packets
{
    /// <summary>
    /// A simple interface used as a basis for all the opcode structure declarations.
    /// Each implementation of this interface must flag the properties with
    /// <see cref="Parsing.IgnoreAttribute"/> for proper deserialization.
    /// This is to avoid the deserializer trying to pick up these properties
    /// as part of the payloads.
    /// </summary>
    public interface IPacketStruct
    {
        DateTime Date { get; set; }
        uint ConnectionID { get; set; }
    }
}
